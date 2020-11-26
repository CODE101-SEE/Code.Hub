using Code.Hub.Core.Services.Base;
using Code.Hub.Core.Services.Epics;
using Code.Hub.Core.WorkProviders.DevOps;
using Code.Hub.Core.WorkProviders.Zammad;
using Code.Hub.EFCoreData;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Extensions;
using Code.Hub.Shared.Models;
using Code.Hub.Shared.WorkProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Code.Hub.Core.Services.WorkLogs
{
    public class WorkLogsService : CodeHubBaseService, IWorkLogsService
    {
        private readonly CodeHubContext _context;
        private readonly IEpicsService _epicsService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDevOpsManager _devOpsManager;
        private readonly IZammadManager _zammadManager;
        private readonly UserManager<CodeHubUser> _userManager;

        public WorkLogsService(CodeHubContext context, IHttpContextAccessor httpContextAccessor, IDevOpsManager devOpsManager, UserManager<CodeHubUser> userManager, IServiceProvider provider, IEpicsService epicsService, IZammadManager zammadManager) : base(provider)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _devOpsManager = devOpsManager;
            _userManager = userManager;
            _epicsService = epicsService;
            _zammadManager = zammadManager;
        }

        public async Task<List<WorkLog>> GetWorkLogs(GetWorkLogsInput input)
        {
            IQueryable<WorkLog> query = _context.WorkLogs;

            // User Filtering
            if (input.UserId.Length == 0 || input.UserId == "UnAssigned")
                input.UserId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            query = (string.IsNullOrEmpty(input.UserId) || input.UserId == "AllUsers") ? query : query.Where(s => s.UserId == input.UserId);

            // CodeHub Filtering
            query = BuildCodeHubQuery(input, query);

            // Generic Filtering
            query = (input.OrganizationId == 0) ? query : query.Where(s => s.OrganizationId == input.OrganizationId);
            query = string.IsNullOrEmpty(input.DescriptionFilter) ? query : query.Where(s => s.Description.Contains(input.DescriptionFilter));

            // Date Filtering
            query = query.Where(s => (s.DateStarted.Date >= input.SearchStartDate.Date || s.DateFinished.Date >= input.SearchStartDate.Date) && (s.DateStarted.Date <= input.SearchEndDate.Date || s.DateFinished.Date <= input.SearchEndDate.Date));
            query = query.OrderBy(s => s.DateStarted.Date);

            return await query.ToListAsync();
        }

        public IQueryable<WorkLog> BuildCodeHubQuery(GetWorkLogsInput input, IQueryable<WorkLog> query)
        {
            //query = (!input.IncludeEpics && !input.IncludeProjects && !input.IncludeOrganizations) ? query : query.Include(log => log.Epic ?? new Epic()).ThenInclude(epic => epic.Project ?? new Project()).ThenInclude(project => project.Organization ?? new Organization());
            query = (!input.IncludeEpics && !input.IncludeProjects && !input.IncludeOrganizations) ? query : query.Include(log => log.Epic.Project.Organization);
            query = (input.EpicId == 0) ? query : query.Where(s => s.Epic != null).Where(s => s.EpicId == input.EpicId);

            query = (input.ProjectId == 0) ? query : query.Where(s => s.Epic != null).Where(s => s.Epic.ProjectId == input.ProjectId);

            return query;
        }

        public async Task<double> GetHoursWorked(List<long> taskIds)
        {
            try
            {
                var hours = await _context.WorkLogs.Where(s => s.TaskId.HasValue && taskIds.Contains((long)s.TaskId)).Select(s => s.Hours).ToListAsync();
                return hours.Sum();

            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to get hours worked for: {taskIds.ToArray()}, e");
                throw;
            }
        }

        public async Task<WorkLog> GetWorkLogOrNull(int id)
        {
            return await _context.WorkLogs.Include(s => s.Epic.Project.Organization).FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<WorkLog> GetWorkLogForEdit(int id)
        {
            var found = await GetWorkLogOrNull(id) ?? new WorkLog
            {
                Description = "",
                EpicId = 0,
                DateStarted = DateTime.UtcNow,
                DateFinished = DateTime.UtcNow,
            };

            return found;
        }

        public Epic GetLastUsedEpicForUser()
        {
            var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            WorkLog lastWorkLog;
            Epic epic;

            try
            {
                lastWorkLog = _context.WorkLogs.OrderByDescending(s => s.DateStarted).FirstOrDefault(s => s.UserId == userId);
                epic = _context.Epics.Include(s => s.Project).ThenInclude(s => s.Organization).FirstOrDefault(s => s.Id == lastWorkLog.EpicId);
            }
            catch
            {
                epic = null;
            }

            return epic;
        }

        public async Task<string> FixDevOpsIds()
        {
            var workLogs = await _context.WorkLogs.Where(s => s.Epic != null && string.IsNullOrWhiteSpace(s.ProviderType)).Include(s => s.Epic.Project.Organization).ToListAsync();
            var logsForUpdate = new List<WorkLog>();

            foreach (var workLog in workLogs)
            {
                workLog.OrganizationId = workLog.Epic.Project.OrganizationId;

                var isFromTask = int.TryParse(workLog.Description, out var taskId);
                if (!isFromTask)
                {
                    workLog.ProviderType = StaticWorkProviderTypes.CodeHub;
                    workLog.TaskId = 0;
                    logsForUpdate.Add(workLog);
                }
                else
                {
                    workLog.TaskId = taskId;
                    workLog.ProviderType = StaticWorkProviderTypes.DevOps;
                    workLog.EpicId = null;
                    workLog.Epic = null;
                }

                logsForUpdate.Add(workLog);
            }

            if (logsForUpdate.Count > 0)
                _context.WorkLogs.UpdateRange(logsForUpdate);

            await _context.SaveChangesAsync();

            var message = $"Found a total of {workLogs.Count} tasks. Managed to get task id for {logsForUpdate.Count} logs.";
            Logger.LogInformation(message);
            return message;
        }

        public async Task<WorkLog> CreateOrEditWorkLog(WorkLog workLog)
        {
            workLog.DateFinished = workLog.DateStarted;

            if (workLog.ProviderType == StaticWorkProviderTypes.CodeHub)
            {
                if (workLog.EpicId.HasValue && workLog.EpicId != 0) // Handle CodeHub Item
                    workLog.Epic = await _context.Epics.FirstOrDefaultAsync(s => s.Id == workLog.EpicId);

                if (workLog.Epic == null)
                    throw new Exception($"Epic not found!");
            }
            else if (workLog.ProviderType == StaticWorkProviderTypes.DevOps || workLog.ProviderType == StaticWorkProviderTypes.Zammad)
            {
                if (workLog.OrganizationId == 0 || workLog.TaskId == 0)
                    throw new Exception($"If epic is not set, you must provide Provider Type, OrganizationId and TaskId");
                // Make sure this is not set, or remove it if set
                workLog.Epic = null;
                workLog.EpicId = null;
            }
            else
            {
                throw new Exception($"Unsupported Work Provider!");
            }

            return (workLog.Id == 0) ? await CreateWorkLog(workLog) : await UpdateWorkLog(workLog);
        }

        public async Task<WorkLog> CreateWorkLog(WorkLog workLog)
        {
            workLog.SubmittedTime = DateTime.Now;

            workLog.UserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _context.WorkLogs.AddAsync(workLog);

            await _context.SaveChangesAsync();

            return workLog;
        }

        public async Task<WorkLog> UpdateWorkLog(WorkLog workLog)
        {
            workLog.LastModifiedTime = DateTime.Now;

            _context.WorkLogs.Update(workLog);

            await _context.SaveChangesAsync();

            return workLog;
        }

        public async Task<bool> DeleteWorkLog(int id)
        {
            try
            {
                var workLog = await _context.WorkLogs.FindAsync(id);
                _context.WorkLogs.Remove(workLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<byte[]> GetDataForExport(List<WorkLog> workLogs)
        {
            var workIds = workLogs.Select(s => s.UserId).ToList();
            var users = await _userManager.Users.Where(s => workIds.Contains(s.Id)).ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("WorkLogs");

            var logs = from log in workLogs
                       select new
                       {
                           User = users.FirstOrDefault(s => log.UserId == s.Id)?.Email ?? "Unknown User",
                           Organization = log.Epic.Project.Organization.Name,
                           Project = log.Epic.Project.Name,
                           Epic = log.Epic.Name,
                           log.Hours,
                           log.Status,
                           log.Description,
                           Date = log.DateStarted.Date,
                       };

            worksheet.Cells["A1:A1"].LoadFromCollection(logs, true);

            worksheet.DefaultColWidth = 25;
            worksheet.Cells[2, 6, workLogs.Count + 1, 10].Style.Numberformat.Format = "dd/mm/yyyy ddd";

            return await package.GetAsByteArrayAsync();
        }

        public async Task<string> ImportFromExcel(string data)
        {
            var filePath = FileHelpers.SaveTempFile(data, ".xlsx");
            var message = await ProcessExcelFile(filePath);

            FileHelpers.DeleteFile(filePath);
            return message;
        }


        private async Task<string> ProcessExcelFile(string path)
        {
            var processed = 0;
            var errors = 0;

            var file = new FileInfo(path);
            var epics = await _epicsService.GetEpics(new GetEpicsInput());
            var users = await _userManager.Users.ToListAsync();

            using var package = new ExcelPackage(file);

            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.End.Row;
            for (var row = 2; row <= rowCount; row++)
            {
                try
                {
                    var userMailString = worksheet.Cells[row, 1].Value.ToString()?.Trim();
                    var epicString = worksheet.Cells[row, 2].Value.ToString()?.Trim();
                    var dateString = worksheet.Cells[row, 3].Value.ToString()?.Trim();
                    var hourString = worksheet.Cells[row, 4].Value.ToString()?.Trim();
                    var desc = worksheet.Cells[row, 5].Value.ToString()?.Trim();

                    await ProcessImportRow(userMailString, epicString, dateString, hourString, desc, epics, users);
                    processed++;

                }
                catch (Exception e)
                {
                    Logger.LogWarning(e.ToString());
                    errors++;
                }
            }

            return $"Rows imported: {processed} rows, errors found: {errors}";
        }

        public async Task ProcessImportRow(string userMailString, string epicString, string dateString, string hourString, string desc, List<Epic> epics, List<CodeHubUser> users)
        {
            var workLog = new WorkLog
            {
                UserId = users.FirstOrDefault(s => s.Email == userMailString)?.Id ?? throw new Exception($"User {userMailString} not found"),
                DateFinished = DateTime.FromOADate(int.Parse(dateString)).Date,
                DateStarted = DateTime.FromOADate(int.Parse(dateString)).Date,
                Description = desc,
                Epic = epics.FirstOrDefault(s => s.Name == epicString) ?? throw new Exception($"Epic {epicString} not found"),
                EpicId = epics.FirstOrDefault(s => s.Name == epicString)?.Id ?? throw new Exception($"Epic {epicString} not found"),
                Hours = DateTimeHelpers.GetTimeFromString(hourString),
                Status = WorkLogStatus.Unbillable,
                SubmittedTime = DateTime.UtcNow
            };

            await CreateWorkLog(workLog);
        }

        public bool WorkLogExists(int id)
        {
            return _context.WorkLogs.Any(e => e.Id == id);
        }

        public async Task<CodeHubWorkItemList> GetAllAvailableWorkItemsFromWorkProviders(bool clearCache)
        {
            var items = new CodeHubWorkItemList { WorkItems = new List<CodeHubWorkItem> { } };
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var devOpsItems = await _devOpsManager.GetAllWorkItemsFromCache(clearCache);
            items.WorkItems.AddRange(devOpsItems.WorkItems);

            var zammadTickets = await _zammadManager.GetAllWorkItemsFromCache(clearCache);

            items.WorkItems.AddRange(zammadTickets.WorkItems);
            stopwatch.Stop();

            items.WorkItems = items.WorkItems.OrderBy(s => s.ChangedDate).ToList();

            Logger.LogDebug($"Received {devOpsItems.WorkItems.Count} DevOps Work Items and {zammadTickets.WorkItems.Count} Zammad Tickets. Took: {stopwatch.ElapsedMilliseconds}ms");

            return items;
        }
    }
}

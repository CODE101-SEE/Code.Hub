using Code.Hub.Core.Services.Base;
using Code.Hub.EFCoreData;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Hub.Core.Services.Epics
{
    public class EpicsService : CodeHubBaseService, IEpicsService
    {
        private readonly CodeHubContext _context;

        public EpicsService(CodeHubContext context, IServiceProvider provider) : base(provider)
        {
            _context = context;
        }

        public async Task<List<Epic>> GetEpics(GetEpicsInput input)
        {
            IQueryable<Epic> query = _context.Epics.OrderBy(s => s.Project.Organization.Name).ThenBy(s => s.Project.Name).ThenBy(s => s.Name);

            query = (!input.IncludeProjects) ? query : query.Include(s => s.Project);

            query = (!input.IncludeOrganizations) ? query : query.Include(s => s.Project).ThenInclude(s => s.Organization);

            query = (input.ProjectId == 0) ? query : query.Where(s => s.ProjectId == input.ProjectId);

            query = (input.OrganizationId == 0) ? query : query.Where(s => s.Project.OrganizationId == input.OrganizationId);

            query = string.IsNullOrEmpty(input.NameFilter) ? query : query.Where(s => s.Name.Contains(input.NameFilter));

            query = (input.IncludeDisabled) ? query : query.Where(s => s.IsDisabled == false);

            return await query.ToListAsync();
        }

        public async Task<Epic> GetEpicOrNull(int id)
        {
            var found = await _context.Epics.FindAsync(id) ?? new Epic { Name = "", Description = "", ProjectId = 0, WorkLogs = new List<WorkLog>() };

            return found;
        }

        public async Task<Epic> GetEpicByNameOrNull(string name)
        {
            return await _context.Epics.FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task<Epic> CreateOrEdit(Epic epic)
        {
            return (epic.Id == 0) ? await CreateEpic(epic) : await UpdateEpic(epic);
        }

        public async Task<Epic> CreateEpic(Epic epic)
        {
            await _context.Epics.AddAsync(epic);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EpicExists(epic.Id))
                {
                    throw new System.Exception("Epic not found");
                }
                else
                {
                    throw;
                }
            }

            return epic;
        }

        public async Task<Epic> UpdateEpic(Epic epic)
        {
            _context.Epics.Update(epic);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EpicExists(epic.Id))
                    throw new Exception("Epic not found");
            }
            return epic;
        }


        public async Task<bool> DeleteEpic(int id)
        {
            try
            {
                var epic = await _context.Epics.FindAsync(id);

                if (_context.WorkLogs.Any(s => s.EpicId == id))
                    return false;

                _context.Epics.Remove(epic);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.Write(e);
                return false;
            }

            return true;
        }

        public bool EpicExists(int id)
        {
            return _context.Epics.Any(e => e.Id == id);
        }
    }
}

using Code.Hub.Core.Dependency;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Models;
using Code.Hub.Shared.WorkProviders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.Core.Services.WorkLogs
{
    public interface IWorkLogsService : IScopedDependency
    {
        Task<List<WorkLog>> GetWorkLogs(GetWorkLogsInput input);
        Task<double> GetHoursWorked(List<long> taskIds);
        Task<WorkLog> GetWorkLogOrNull(int id);
        Epic GetLastUsedEpicForUser();
        Task<WorkLog> CreateWorkLog(WorkLog workLog);
        Task<WorkLog> UpdateWorkLog(WorkLog workLog);
        Task<bool> DeleteWorkLog(int id);
        Task<byte[]> GetDataForExport(List<WorkLog> workLogs);
        bool WorkLogExists(int id);
        Task<string> ImportFromExcel(string data);
        Task<CodeHubWorkItemList> GetAllAvailableWorkItemsFromWorkProviders(bool clearCache);
        Task<WorkLog> CreateOrEditWorkLog(WorkLog workLog);
        Task<WorkLog> GetWorkLogForEdit(int id);
        Task<string> FixDevOpsIds();
    }
}
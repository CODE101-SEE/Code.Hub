using Code.Hub.Core.Dependency;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.Core.Services.Epics
{
    public interface IEpicsService : IScopedDependency
    {
        Task<List<Epic>> GetEpics(GetEpicsInput input);
        Task<Epic> GetEpicOrNull(int id);
        Task<Epic> GetEpicByNameOrNull(string name);
        Task<Epic> CreateOrEdit(Epic epic);
        Task<Epic> CreateEpic(Epic epic);
        Task<Epic> UpdateEpic(Epic epic);
        Task<bool> DeleteEpic(int id);
        bool EpicExists(int id);
    }
}
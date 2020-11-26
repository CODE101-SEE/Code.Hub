using Code.Hub.Core.Dependency;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.Core.Services.Projects
{
    public interface IProjectsService : IScopedDependency
    {
        Task<List<Project>> GetProjects(GetProjectsInput input);
        Task<Project> GetProjectOrNull(int id);
        Task<Project> GetProjectForEdit(int id);
        Task<Project> CreateProject(Project project);
        Task<Project> CreateOrUpdateProject(Project project);
        Task<Project> UpdateProject(Project project);
        Task<bool> DeleteProject(int id);
        bool ProjectExists(int id);
    }
}
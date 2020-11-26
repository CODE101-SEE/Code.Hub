using Code.Hub.Core.Dependency;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.Core.Services.Users
{
    public interface IUserService : IScopedDependency
    {
        Task<CreateOrEditUserInput> GetUserForEdit(string id);
        Task<bool> CreateOrEditUser(CreateOrEditUserInput input);
        Task UpdateLastLoginTime(CodeHubUser user);
        Task<string> GetCurrentUserEmail();
        Task<string> GetCurrentUserId();
        Task<CodeHubUser> GetCurrentUser();
        Task<List<CodeHubUser>> GetAllOrderedUsers();
        Task<bool> IsUserAdmin();
        List<string> GetAdmins();
    }
}
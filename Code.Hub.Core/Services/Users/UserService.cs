using Code.Hub.Core.Services.Base;
using Code.Hub.EFCoreData;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Code.Hub.Core.Services.Users
{
    public class UserService : CodeHubBaseService, IUserService
    {
        private readonly CodeHubContext _context;
        public readonly UserManager<CodeHubUser> UserManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(CodeHubContext context, UserManager<CodeHubUser> userManager, IServiceProvider provider, IHttpContextAccessor httpContextAccessor) : base(provider)
        {
            _context = context;
            UserManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> IsUserAdmin()
        {
            var user = await GetCurrentUserEmail();
            return GetAdmins().Contains(user);
        }

        public List<string> GetAdmins()
        {
            return new List<string>
            {
                "antonio.mikulic@smart-digital.hr", "antonio.mikulic@code101.eu",
                "igor.marinic@code101.eu", "igor@smart-digital.hr",
                "marko.horvat@code101.eu", "robert@smart-digital.hr", "anja.tomas@code101.eu",
            };
        }

        public async Task<CreateOrEditUserInput> GetUserForEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Logger.LogInformation("Creating a new user!");
                return new CreateOrEditUserInput { LoginEnabled = true, Comment = "" };
            }

            var user = await UserManager.FindByIdAsync(id);
            Logger.LogInformation($"Started editing user {user.Email}!");

            return ObjectMapper.Map<CreateOrEditUserInput>(user);
        }

        public async Task<bool> CreateOrEditUser(CreateOrEditUserInput input)
        {
            if (string.IsNullOrEmpty(input.Id))
                return await CreateUser(input);

            return await UpdateUser(input);
        }

        private async Task<bool> CreateUser(CreateOrEditUserInput input)
        {
            var user = ObjectMapper.Map<CodeHubUser>(input);
            user.Id = Guid.NewGuid().ToString();
            var result = await UserManager.CreateAsync(user, input.Password);

            return result.Succeeded;
        }

        private async Task<bool> UpdateUser(CreateOrEditUserInput input)
        {
            try
            {
                var user = await UserManager.FindByIdAsync(input.Id);
                ObjectMapper.Map(input, user);

                var result = await UserManager.UpdateAsync(user);

                if (!input.RequestPasswordChange) return result.Succeeded;

                var token = await UserManager.GeneratePasswordResetTokenAsync(user);

                var passwordChangeResult = await UserManager.ResetPasswordAsync(user, token, input.Password);

                return passwordChangeResult.Succeeded && result.Succeeded;

            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task UpdateLastLoginTime(CodeHubUser user)
        {
            user.LastLoggedInTime = DateTime.UtcNow;
            await UserManager.UpdateAsync(user);
        }

        public async Task<List<CodeHubUser>> GetAllOrderedUsers()
        {
            return await UserManager.Users.OrderBy(s => s.Email).ToListAsync();
        }

        public async Task<string> GetCurrentUserEmail()
        {
            var user = await GetCurrentUser();
            return user.Email;
        }

        public async Task<string> GetCurrentUserId()
        {
            var user = await GetCurrentUser();
            return user.Id;
        }

        public async Task<CodeHubUser> GetCurrentUser()
        {
            var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }
    }

}

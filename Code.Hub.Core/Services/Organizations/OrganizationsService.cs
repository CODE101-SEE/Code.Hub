using Code.Hub.Core.Services.Base;
using Code.Hub.EFCoreData;
using Code.Hub.Shared.Models;
using Code.Hub.Shared.WorkProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Hub.Core.Services.Organizations
{
    public class OrganizationsService : CodeHubBaseService, IOrganizationsService
    {
        private readonly CodeHubContext _context;

        public OrganizationsService(CodeHubContext context, IServiceProvider provider) : base(provider)
        {
            _context = context;
        }

        public async Task<List<Organization>> GetOrganizations()
        {
            return await _context.Organizations.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<List<Organization>> GetWorkProviderOrganizations()
        {
            return await _context.Organizations.OrderBy(s => s.Name)
                .Where(s => !string.IsNullOrEmpty(s.ProviderType) && !s.ProviderType.Equals(StaticWorkProviderTypes.CodeHub)
                        && !string.IsNullOrEmpty(s.Color) && !string.IsNullOrEmpty(s.AuthToken) && !string.IsNullOrEmpty(s.Url))
                .ToListAsync();
        }

        public async Task<Organization> GetOrganizationForEdit(int id)
        {
            var organization = await _context.Organizations.FindAsync(id) ??
                               new Organization { Name = "", Description = "", Projects = new List<Project>() };

            organization.ProviderType = string.IsNullOrWhiteSpace(organization.ProviderType) ? StaticWorkProviderTypes.CodeHub : organization.ProviderType;

            return organization;
        }

        public async Task<Organization> CreateOrEditOrganization(Organization organization)
        {
            if (string.IsNullOrEmpty(organization.ProviderType)) throw new Exception("Provider type is required!");
            return (organization.Id != 0) ? await UpdateOrganization(organization) : await CreateOrganization(organization);
        }

        public async Task<Organization> GetOrganization(int id)
        {
            var organization = await _context.Organizations.Include(s => s.Projects).ThenInclude(p => p.Epics).FirstOrDefaultAsync(s => s.Id == id);

            if (organization == null)
                throw new Exception("Organization not found");

            return organization;
        }

        public async Task<Organization> CreateOrganization(Organization organization)
        {
            await _context.Organizations.AddAsync(organization);

            await SaveInternal(organization.Id);
            return organization;
        }

        private async Task SaveInternal(int id)
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrganizationExists(id))
                    throw new Exception("Organization not found");
            }
        }

        public async Task<Organization> UpdateOrganization(Organization organization)
        {
            _context.Organizations.Update(organization);

            await SaveInternal(organization.Id);

            return organization;
        }

        public async Task<bool> DeleteOrganization(int id)
        {
            try
            {
                var org = await _context.Organizations.FindAsync(id);

                if (_context.Projects.Any(s => s.OrganizationId == id))
                    return false;

                _context.Organizations.Remove(org);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.LogWarning(e.ToString());
            }

            return true;
        }

        public bool OrganizationExists(int id)
        {
            return _context.Organizations.Any(e => e.Id == id);
        }
    }
}

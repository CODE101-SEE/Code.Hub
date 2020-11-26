using Code.Hub.Core.Dependency;
using Code.Hub.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.Core.Services.Organizations
{
    public interface IOrganizationsService : IScopedDependency
    {
        Task<List<Organization>> GetOrganizations();
        Task<Organization> GetOrganization(int id);
        Task<Organization> GetOrganizationForEdit(int id);
        Task<Organization> CreateOrganization(Organization organization);
        Task<Organization> CreateOrEditOrganization(Organization organization);
        Task<Organization> UpdateOrganization(Organization organization);
        Task<List<Organization>> GetWorkProviderOrganizations();
        Task<bool> DeleteOrganization(int id);
        bool OrganizationExists(int id);
    }
}
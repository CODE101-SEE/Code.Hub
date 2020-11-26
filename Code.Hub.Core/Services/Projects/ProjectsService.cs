using Code.Hub.Core.Services.Base;
using Code.Hub.Core.Services.Organizations;
using Code.Hub.EFCoreData;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Hub.Core.Services.Projects
{
    public class ProjectsService : CodeHubBaseService, IProjectsService
    {
        private readonly CodeHubContext _context;
        private readonly IOrganizationsService _organizationsService;

        public ProjectsService(CodeHubContext context, IServiceProvider serviceProvider, IOrganizationsService organizationsService) : base(serviceProvider)
        {
            _context = context;
            _organizationsService = organizationsService;
        }

        public async Task<List<Project>> GetProjects(GetProjectsInput input)
        {
            IQueryable<Project> query = _context.Projects.OrderBy(s => s.Organization.Name).ThenBy(s => s.Name);

            query = (!input.IncludeOrganizations) ? query : query.Include(s => s.Organization);

            query = (input.OrganizationId == 0) ? query : query.Where(s => s.OrganizationId == input.OrganizationId);

            query = string.IsNullOrEmpty(input.NameFilter) ? query : query.Where(s => s.Name.Contains(input.NameFilter));

            query = (input.IncludeDisabled) ? query : query.Where(s => s.IsDisabled == false);

            return await query.ToListAsync();
        }

        public async Task<Project> GetProjectOrNull(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task<Project> GetProjectForEdit(int id)
        {
            var project = await GetProjectOrNull(id) ?? new Project
            {
                Name = "",
                Description = "",
                OrganizationId = 0,
                IsDisabled = false,
                Epics = new List<Epic>()
            };

            return project;
        }

        public async Task<Project> CreateProject(Project project)
        {
            if (ProjectExists(project.Id))
                throw new Exception("Project already exists");

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            return project;
        }

        public async Task<Project> CreateOrUpdateProject(Project project)
        {
            if (_organizationsService.OrganizationExists(project.OrganizationId))
                project.Organization = await _organizationsService.GetOrganization(project.OrganizationId);

            return project.Id == 0 ? await CreateProject(project) : await UpdateProject(project);
        }

        public async Task<Project> UpdateProject(Project project)
        {
            if (!ProjectExists(project.Id))
            {
                throw new System.Exception("Project not found");
            }

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();

            return project;
        }


        public async Task<bool> DeleteProject(int id)
        {
            try
            {
                var proj = await _context.Projects.FindAsync(id);

                if (_context.Epics.Any(s => s.ProjectId == id))
                    return false;

                _context.Projects.Remove(proj);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.Write(e);
                return false;
            }

            return true;
        }

        public bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}

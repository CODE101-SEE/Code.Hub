using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Code.Hub.Core.Caches;
using Code.Hub.Core.Services.Organizations;
using Code.Hub.Shared.Models;
using Code.Hub.Shared.WorkProviders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Zammad.Client;
using Zammad.Client.Core;

namespace Code.Hub.Core.WorkProviders.Providers
{
    public class ZammadWorkProvider : IWorkProvider, IWorkCacheProvider
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ZammadWorkProvider> _logger;
        private readonly ICodeHubCache _codeHubCache;
        private readonly IOrganizationsService _organizationsService;

        public ZammadWorkProvider(ILogger<ZammadWorkProvider> logger, IMapper mapper, ICodeHubCache codeHubCache, IOrganizationsService organizationsService)
        {
            _mapper = mapper;
            _logger = logger;
            _codeHubCache = codeHubCache;
            _organizationsService = organizationsService;
        }

        public async Task<CodeHubWorkItemList> GetAllWorkItems()
        {
            try
            {
                var allWorkItems = new CodeHubWorkItemList { WorkItems = new List<CodeHubWorkItem>() };

                foreach (var organization in await GetProviderOrganizationsAsync())
                {
                    allWorkItems.WorkItems.AddRange((await GetWorkItems(organization)).WorkItems);
                }

                allWorkItems.WorkItems = allWorkItems.WorkItems.OrderByDescending(s => s.ChangedDate).ToList();
                return allWorkItems;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task<CodeHubWorkItemList> GetWorkItems(Organization organization)
        {
            return new CodeHubWorkItemList { WorkItems = await GetZammadItems(organization) };
        }

        public async ValueTask InvalidateAllCacheAsync()
        {
            foreach (var organization in await GetProviderOrganizationsAsync())
            {
                await InvalidateCacheAsync(organization);
            }
        }

        public ValueTask InvalidateCacheAsync(Organization organization)
        { 
            _codeHubCache.Cache.Remove(CreateCacheKey(organization));
            return ValueTask.CompletedTask;
        }

        public async Task<List<CodeHubWorkItem>> GetZammadItems(Organization organization)
        {
            var devOpsWorkItems = await GetAllWorkItemsFromProviderFromCache(organization);

            foreach (var workItem in devOpsWorkItems.WorkItems)
            {
                workItem.Color = organization.Color;
                workItem.ProviderType = StaticWorkProviderTypes.Zammad;
                workItem.ProviderOrganization = organization.Name;
                workItem.OrganizationId = organization.Id;
                workItem.Parent = FillParentProperties(workItem);
            }

            return devOpsWorkItems.WorkItems.OrderBy(p=> p.ChangedDate).ToList();
        }

        private async Task<CodeHubWorkItemList> GetAllWorkItemsFromProviderFromCache(Organization organization)
        {
            var workItems = await _codeHubCache.Cache.GetOrCreateAsync(CreateCacheKey(organization), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(organization.CacheDurationSeconds > 0 ? organization.CacheDurationSeconds : 60 * 60 * 24);
                return await GetAllWorkItemsFromProvider(organization);
            });

            return workItems;
        }

        private ZammadAccount CreateConnection(Organization organization)
        {
            return new ZammadAccount(new Uri(organization.Url), ZammadAuthentication.Token, string.Empty, string.Empty, organization.AuthToken);
        }

        private async Task<CodeHubWorkItemList> GetAllWorkItemsFromProvider(Organization organization)
        {
            try
            {
                var connection = CreateConnection(organization);
                var client = connection.CreateTicketClient();
                var tickets = await client.GetTicketListAsync();
                var ticketList = tickets.ToList();

                var workItems = new CodeHubWorkItemList { WorkItems = _mapper.Map<List<CodeHubWorkItem>>(ticketList) };
                foreach (var ticket in workItems.WorkItems)
                {
                    ticket.Color = organization.Color;
                    ticket.ProviderType = StaticWorkProviderTypes.Zammad;
                    ticket.ProviderOrganization = organization.Name;
                    ticket.OrganizationId = organization.Id;
                    ticket.WorkItemType = "Ticket";
                }

                return workItems;
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception while fetching Zammad items: {e}");
                throw;
            }
        }

        public CodeHubWorkItem FillParentProperties(CodeHubWorkItem workItem)
        {
            if (workItem.Parent == null) return workItem.Parent;

            workItem.Parent.Color = workItem.Color;
            workItem.Parent.ProviderType = workItem.ProviderType;
            workItem.Parent.ProviderOrganization = workItem.ProviderOrganization;
            workItem.Parent.OrganizationId = workItem.Id;

            return workItem.Parent;
        }

        private async Task<IEnumerable<Organization>> GetProviderOrganizationsAsync()
        {
            var workProviders = await _organizationsService.GetWorkProviderOrganizations();
            return workProviders.Where(p => p.ProviderType == WorkProviderType.Zammad && !p.IsDisabled);
        }

        private string CreateCacheKey(Organization organization)
        {
            return $"ZammadWorkItemsFromProvider{organization.Url}";
        }
    }
}

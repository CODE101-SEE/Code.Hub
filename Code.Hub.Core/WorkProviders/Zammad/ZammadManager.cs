using AutoMapper;
using Code.Hub.Core.Caches;
using Code.Hub.Core.Services.Organizations;
using Code.Hub.Shared.Models;
using Code.Hub.Shared.WorkProviders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zammad.Client;
using Zammad.Client.Core;

namespace Code.Hub.Core.WorkProviders.Zammad
{
    public class ZammadManager : IZammadManager
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ZammadManager> _logger;
        private readonly ICodeHubCache _codeHubCache;
        private readonly IOrganizationsService _organizationsService;

        public ZammadManager(IMapper mapper, ILogger<ZammadManager> logger, ICodeHubCache codeHubCache, IOrganizationsService organizationsService)
        {
            _mapper = mapper;
            _logger = logger;
            _codeHubCache = codeHubCache;
            _organizationsService = organizationsService;
        }

        private ZammadAccount CreateConnectionFromConfiguration(Organization organization)
        {
            return new ZammadAccount(new Uri(organization.Url), ZammadAuthentication.Token, string.Empty, string.Empty, organization.AuthToken);
        }

        public async Task<CodeHubWorkItemList> GetAllWorkItemsFromCache(bool clearCache)
        {
            try
            {
                var workProviders = await _organizationsService.GetWorkProviderOrganizations();

                var allWorkItems = new CodeHubWorkItemList { WorkItems = new List<CodeHubWorkItem>() };

                foreach (var organization in workProviders.Where(organization =>
                    organization.ProviderType == StaticWorkProviderTypes.Zammad))
                    allWorkItems.WorkItems.AddRange(await GetZammadItems(organization, clearCache));

                allWorkItems.WorkItems = allWorkItems.WorkItems.OrderByDescending(s => s.ChangedDate).ToList();
                return allWorkItems;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public async Task<List<CodeHubWorkItem>> GetZammadItems(Organization organization, bool clearCache)
        {
            var devOpsWorkItems = await GetAllWorkItemsFromProviderFromCache(organization, clearCache);

            foreach (var workItem in devOpsWorkItems.WorkItems)
            {
                workItem.Color = organization.Color;
                workItem.ProviderType = StaticWorkProviderTypes.Zammad;
                workItem.ProviderOrganization = organization.Name;
                workItem.OrganizationId = organization.Id;
                workItem.Parent = FillParentProperties(workItem);
            }

            return devOpsWorkItems.WorkItems;
        }

        private async Task<CodeHubWorkItemList> GetAllWorkItemsFromProviderFromCache(Organization organization, bool clearCache)
        {
            var cacheKey = $"ZammadWorkItemsFromProvider{organization.Url}";
            if (clearCache)
                _codeHubCache.Cache.Remove(cacheKey);

            var workItems = await _codeHubCache.Cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(organization.CacheDurationSeconds > 0 ? organization.CacheDurationSeconds : 60 * 60 * 24);
                return await GetAllWorkItemsFromProvider(organization);
            });

            return workItems;
        }

        private async Task<CodeHubWorkItemList> GetAllWorkItemsFromProvider(Organization organization)
        {
            try
            {
                var connection = CreateConnectionFromConfiguration(organization);
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
    }
}

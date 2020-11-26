using AutoMapper;
using Code.Hub.Core.Caches;
using Code.Hub.Core.Services.Organizations;
using Code.Hub.Shared.Configurations.DevOps;
using Code.Hub.Shared.Models;
using Code.Hub.Shared.WorkProviders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Hub.Core.WorkProviders.DevOps
{
    public class DevOpsManager : IDevOpsManager
    {
        private readonly IOptions<DevOpsConfiguration> _devOpsConfiguration;
        private readonly IMapper _mapper;
        private readonly ILogger<DevOpsManager> _logger;
        private readonly ICodeHubCache _codeHubCache;
        private readonly IOrganizationsService _organizationsService;

        public DevOpsManager(IOptions<DevOpsConfiguration> devOpsConfiguration, IMapper mapper, ILogger<DevOpsManager> logger, ICodeHubCache codeHubCache, IOrganizationsService organizationsService)
        {
            _devOpsConfiguration = devOpsConfiguration;
            _mapper = mapper;
            _logger = logger;
            _codeHubCache = codeHubCache;
            _organizationsService = organizationsService;
        }

        private VssConnection CreateConnection()
        {
            return CreateConnectionFromConfiguration(_devOpsConfiguration.Value);
        }

        private VssConnection CreateConnectionFromConfiguration(DevOpsConfiguration configuration)
        {
            return new VssConnection(new Uri(configuration.OrganizationUrl), new VssBasicCredential(string.Empty, configuration.OrganizationOwnerPatToken));
        }


        public async Task<CodeHubWorkItemList> GetAllWorkItemsFromCache(bool clearCache)
        {
            var workProviders = await _organizationsService.GetWorkProviderOrganizations();

            var allWorkItems = new CodeHubWorkItemList { WorkItems = new List<CodeHubWorkItem>() };
            foreach (var organization in workProviders.Where(organization => organization.ProviderType == StaticWorkProviderTypes.DevOps))
                allWorkItems.WorkItems.AddRange(await GetDevOpsItems(organization, clearCache));

            return allWorkItems;
        }

        public async Task<CodeHubWorkItemList> GetWorkItemsFromProviderFromCache(DevOpsConfiguration configuration, bool clearCache = false)
        {
            var cacheKey = $"WorkItemFromProviderGroups{configuration.OrganizationUrl}";
            if (clearCache)
                _codeHubCache.Cache.Remove(cacheKey);

            var workItems = await _codeHubCache.Cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_devOpsConfiguration.Value.CacheDuration);
                return await GetWorkItemsFromProvider(configuration);
            });

            return workItems;
        }

        public async Task<CodeHubWorkItemList> GetWorkItemTreeFromProviderFromCache(DevOpsConfiguration configuration, bool clearCache = false)
        {
            var cacheKey = $"GetWorkItemTreeFromProvider{configuration.OrganizationUrl}";
            if (clearCache)
                _codeHubCache.Cache.Remove(cacheKey);

            var workItems = await _codeHubCache.Cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_devOpsConfiguration.Value.CacheDuration);
                return await GetWorkItemTreeFromProvider(configuration);
            });

            return workItems;
        }

        public async Task<CodeHubWorkItemList> GetWorkItemTreeFromProvider(DevOpsConfiguration configuration)
        {
            configuration.WorkItemQuery = configuration.AllWorkItemsQuery;
            var connection = CreateConnectionFromConfiguration(configuration);

            var client = connection.GetClient<WorkItemTrackingHttpClient>();

            var workItems = await GetWorkItemGroupsInternal(client, configuration);
            return BuildTree(workItems);
        }

        public CodeHubWorkItemList BuildTree(List<WorkItem> workItems)
        {
            try
            {
                var childWorkItemTypes = new[] { "User Story", "Bug" };
                var epics = workItems.Where(s => (string)s.Fields[StaticDevOpsWorkItemFields.WorkItemType] == "Epic").ToList();
                var features = workItems.Where(s => (string)s.Fields[StaticDevOpsWorkItemFields.WorkItemType] == "Feature").ToList();
                var childWorkItems = workItems.Where(s => childWorkItemTypes.Contains(s.Fields[StaticDevOpsWorkItemFields.WorkItemType])).ToList();

                var codeHubWorkItems = new List<CodeHubWorkItem>();
                foreach (var epic in epics)
                {
                    var epicItem = _mapper.Map<CodeHubWorkItem>(epic);

                    var epicChildren = features.Where(s => s.Fields.ContainsKey(StaticDevOpsWorkItemFields.Parent) && (long)s.Fields[StaticDevOpsWorkItemFields.Parent] == epicItem.Id).ToList();
                    var featureItems = _mapper.Map<List<CodeHubWorkItem>>(epicChildren);

                    foreach (var featureItem in featureItems)
                    {
                        featureItem.Parent = epicItem;

                        var featureChildren = childWorkItems.Where(s => s.Fields.ContainsKey(StaticDevOpsWorkItemFields.Parent) && (long)s.Fields[StaticDevOpsWorkItemFields.Parent] == featureItem.Id).ToList();
                        var codeHubItems = _mapper.Map<List<CodeHubWorkItem>>(featureChildren);

                        foreach (var item in codeHubItems)
                            item.Parent = featureItem;

                        codeHubWorkItems.AddRange(codeHubItems);
                    }
                }

                return new CodeHubWorkItemList { WorkItems = codeHubWorkItems };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<CodeHubWorkItemList> GetWorkItemsFromProvider(DevOpsConfiguration configuration)
        {
            var connection = CreateConnectionFromConfiguration(configuration);
            var client = connection.GetClient<WorkItemTrackingHttpClient>();

            return await GetAllWorkItemsInternal(client, null);
        }

        private async Task<CodeHubWorkItemList> GetAllWorkItems()
        {
            var client = CreateConnection().GetClient<WorkItemTrackingHttpClient>();
            return await GetAllWorkItemsInternal(client, null);
        }

        private async Task<CodeHubWorkItemList> GetAllWorkItemsInternal(WorkItemTrackingHttpClient client, DevOpsConfiguration configuration)
        {
            configuration ??= _devOpsConfiguration.Value;

            var allItems = await GetWorkItemGroupsInternal(client, configuration);

            var mapped = _mapper.Map<List<CodeHubWorkItem>>(allItems);

            return new CodeHubWorkItemList { WorkItems = mapped };
        }

        private async Task<List<WorkItem>> GetWorkItemGroupsInternal(WorkItemTrackingHttpClient client, DevOpsConfiguration configuration)
        {
            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var allItems = new List<WorkItem>();

                var query = new Wiql { Query = configuration.WorkItemQuery };
                var workItemGroupResult = await client.QueryByWiqlAsync(query);
                var workItemGroups = (from i in Enumerable.Range(0, workItemGroupResult.WorkItems.Count()) group workItemGroupResult.WorkItems.ToList()[i] by i / 200).ToList();

                _logger.LogInformation($"Got {workItemGroupResult.WorkItems.Count()} split into {workItemGroups.Count()} groups. Elapsed: {stopWatch.ElapsedMilliseconds}");
                foreach (var queryGroup in workItemGroups)
                {
                    if (!queryGroup.Any()) continue;

                    var newIds = queryGroup.ToList().Select(s => s.Id).ToArray();
                    List<WorkItem> workItems;

                    if (configuration.GetWorkItemFields().Length == 0)
                        workItems = await client.GetWorkItemsAsync(ids: newIds, asOf: workItemGroupResult.AsOf, expand: WorkItemExpand.Relations);
                    else
                        workItems = await client.GetWorkItemsAsync(ids: newIds, fields: configuration.GetWorkItemFields(), asOf: workItemGroupResult.AsOf);

                    allItems.AddRange(workItems);
                }

                if (!string.IsNullOrEmpty(configuration.OrderByWorkItemFieldName) && configuration.GetWorkItemFields().Contains(configuration.OrderByWorkItemFieldName))
                    allItems = allItems.OrderByDescending(s => s.Fields[configuration.OrderByWorkItemFieldName]).ToList();

                _logger.LogInformation($"Found {allItems.Count} items, Elapsed: {stopWatch.ElapsedMilliseconds}");
                return allItems;
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Failed to fetch DevOp items {e}");
                throw;
            }
        }

        public async Task<List<CodeHubWorkItem>> GetDevOpsItems(Organization organization, bool clearCache)
        {
            var config = _devOpsConfiguration.Value;
            config.OrganizationUrl = organization.Url;
            config.OrganizationOwnerPatToken = organization.AuthToken;

            var devOpsWorkItems = await GetWorkItemTreeFromProviderFromCache(config, clearCache);

            foreach (var workItem in devOpsWorkItems.WorkItems)
            {
                workItem.Color = organization.Color;
                workItem.ProviderType = StaticWorkProviderTypes.DevOps;
                workItem.ProviderOrganization = organization.Name;
                workItem.OrganizationId = organization.Id;
                workItem.Parent = FillParentProperties(workItem);
            }

            return devOpsWorkItems.WorkItems;
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

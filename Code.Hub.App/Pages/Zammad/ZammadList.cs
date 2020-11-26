using Code.Hub.Shared.WorkProviders;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.Zammad
{
    public partial class ZammadList : ComponentBase
    {
        public List<CodeHubWorkItem> WorkItems { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (!(await UserService.IsUserAdmin()))
                UriHelper.NavigateTo(@"\worklogs");

            await GetWorkItems();
        }

        public async Task GetWorkItems()
        {
            var items = await ZammadManager.GetAllWorkItemsFromCache();
            WorkItems = items.WorkItems;
            StateHasChanged();
        }
    }
}

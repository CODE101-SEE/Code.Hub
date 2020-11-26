using Microsoft.AspNetCore.Components;
using System;

namespace Code.Hub.App.Pages.Modal
{
    public class ModalService : IModalService
    {
        public event Action<ModalResult> OnClose;

        internal event Action<string, RenderFragment, ModalParameters> OnShow;

        public void Show(string title, Type componentType)
        {
            Show(title, componentType, new ModalParameters());
        }

        public void Show(string title, Type componentType, ModalParameters parameters)
        {
            if (!typeof(ComponentBase).IsAssignableFrom(componentType))
            {
                throw new ArgumentException($"{componentType.FullName} must be a Blazor Component");
            }

            var content = new RenderFragment(x => { x.OpenComponent(1, componentType); x.CloseComponent(); });

            OnShow?.Invoke(title, content, parameters);
        }

        public void Cancel()
        {
            OnClose?.Invoke(ModalResult.Cancel());
        }

        public void Close(ModalResult modalResult)
        {
            OnClose?.Invoke(modalResult);
        }
    }
}

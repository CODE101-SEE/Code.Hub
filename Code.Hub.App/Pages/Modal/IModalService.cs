using Code.Hub.Core.Dependency;
using System;

namespace Code.Hub.App.Pages.Modal
{
    public interface IModalService : IScopedDependency
    {
        event Action<ModalResult> OnClose;

        void Show(string title, Type contentType);

        void Show(string title, Type contentType, ModalParameters parameters);

        void Cancel();

        void Close(ModalResult modalResult);
    }
}

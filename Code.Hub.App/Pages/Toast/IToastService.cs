using Code.Hub.Core.Dependency;
using System;

namespace Code.Hub.App.Pages.Toast
{
    public interface IToastService : IDisposable, IScopedDependency
    {
        event Action<string, ToastLevel> OnShow;
        event Action OnHide;
        void ShowToast(string message, ToastLevel level);
    }
}
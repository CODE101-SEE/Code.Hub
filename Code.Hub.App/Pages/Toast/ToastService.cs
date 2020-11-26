using System;
using System.Timers;

namespace Code.Hub.App.Pages.Toast
{
    public class ToastService : IToastService
    {
        public event Action<string, ToastLevel> OnShow;
        public event Action OnHide;
        private Timer _countdown;
        public double ToastDisplayMs = 5 * 1000;

        public void ShowToast(string message, ToastLevel level)
        {
            OnShow?.Invoke(message, level);
            StartCountdown();
        }

        private void StartCountdown()
        {
            SetCountdown();

            if (_countdown.Enabled)
            {
                _countdown.Stop();
                _countdown.Start();
            }
            else
                _countdown.Start();
        }

        private void SetCountdown()
        {
            if (_countdown != null) return;
            _countdown = new Timer(ToastDisplayMs);
            _countdown.Elapsed += HideToast;
            _countdown.AutoReset = false;
        }

        private void HideToast(object source, ElapsedEventArgs args)
        {
            OnHide?.Invoke();
        }

        public void Dispose()
        {
            _countdown?.Dispose();
        }
    }
}

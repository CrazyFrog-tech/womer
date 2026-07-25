using System;
using System.Collections.Generic;
using System.Text;
using womer.Core.Interfaces;

namespace womer.Infrastructure.Navigation
{
    public sealed class ShellNavigationService : INavigationService
    {
        private readonly SemaphoreSlim _navigationLock = new(1, 1);
        private string? _lastRoute;
        private DateTime _lastNavigationUtc;

        public async Task<bool> GoToAsync(string route, bool animate = true)
        {
            if (string.IsNullOrWhiteSpace(route))
                return false;

            // Non-blocking: if already navigating, ignore new request
            if (!await _navigationLock.WaitAsync(0))
                return false;

            try
            {
                var now = DateTime.UtcNow;
                var isDuplicateBurst =
                    string.Equals(_lastRoute, route, StringComparison.OrdinalIgnoreCase) &&
                    now - _lastNavigationUtc < TimeSpan.FromMilliseconds(700);

                if (isDuplicateBurst)
                    return false;

                _lastRoute = route;
                _lastNavigationUtc = now;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync(route, animate);
                });

                return true;
            }
            finally
            {
                _navigationLock.Release();
            }
        }
    }
}

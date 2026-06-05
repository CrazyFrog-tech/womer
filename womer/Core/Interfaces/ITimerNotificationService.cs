namespace womer.Core.Interfaces
{
    public interface ITimerNotificationService
    {
        Task EnsurePermissionAsync();
        void StartOrUpdate(string phase, int currentSet, int totalSets, TimeSpan remaining);
        void Stop();
        void SetLockScreenMode(bool enabled);
    }
}

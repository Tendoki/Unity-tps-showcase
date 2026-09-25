using System;

using Game.Services;

namespace Game.Scripts.Services.Messaging
{
    public interface INotificationService : IService
    {
        void Initialize();

        void RequestPermission();

        string Schedule(string title, string message, DateTime fireTime);

        void Cancel(string notificationId);

        void CancelAll();
    }
}

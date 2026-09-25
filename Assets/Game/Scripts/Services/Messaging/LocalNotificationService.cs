using System;
using UnityEngine;

#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace Game.Scripts.Services.Messaging
{
    public sealed class LocalNotificationService : INotificationService
    {
        private const string AndroidChannelId = "game_notifications";

        public void Initialize()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            InitializeAndroid();
#elif UNITY_IOS
            InitializeIos();
#endif
        }

        public void RequestPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            RequestAndroidPermission();
#elif UNITY_IOS
            RequestIosPermission();
#endif
        }

        public string Schedule(
            string title,
            string message,
            DateTime fireTime)
        {
            if (fireTime <= DateTime.Now)
            {
                Debug.LogWarning(
                    $"Cannot schedule notification in the past. FireTime: {fireTime}");

                return null;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            return ScheduleAndroid(title, message, fireTime);
#elif UNITY_IOS
            return ScheduleIos(title, message, fireTime);
#else
            Debug.Log(
                $"Notification scheduled in editor: {title} / {message} / {fireTime}");

            return Guid.NewGuid().ToString();
#endif
        }

        public void Cancel(string notificationId)
        {
            if (string.IsNullOrWhiteSpace(notificationId))
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
            if (int.TryParse(notificationId, out int id))
            {
                AndroidNotificationCenter.CancelScheduledNotification(id);
            }
#elif UNITY_IOS
            iOSNotificationCenter.RemoveScheduledNotification(notificationId);
#endif
        }

        public void CancelAll()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidNotificationCenter.CancelAllScheduledNotifications();
#elif UNITY_IOS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR

        private void InitializeAndroid()
        {
            var channel = new AndroidNotificationChannel
            {
                Id = AndroidChannelId,
                Name = "Game Notifications",
                Description = "Gameplay notifications",
                Importance = Importance.Default
            };

            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        private void RequestAndroidPermission()
        {
            if (GetAndroidSdkVersion() < 33)
                return;

            const string permission =
                "android.permission.POST_NOTIFICATIONS";

            if (Permission.HasUserAuthorizedPermission(permission))
                return;

            Permission.RequestUserPermission(permission);
        }

        private string ScheduleAndroid(
            string title,
            string message,
            DateTime fireTime)
        {
            var notification = new AndroidNotification
            {
                Title = title,
                Text = message,
                FireTime = fireTime
            };

            int id = AndroidNotificationCenter.SendNotification(
                notification,
                AndroidChannelId
            );

            return id.ToString();
        }

        private static int GetAndroidSdkVersion()
        {
            using var version =
                new AndroidJavaClass("android.os.Build$VERSION");

            return version.GetStatic<int>("SDK_INT");
        }

#endif

#if UNITY_IOS
        private void InitializeIos()
        {
            // Для базовой реализации здесь ничего не требуется.
        }

        private async void RequestIosPermission()
        {
            using var request = new AuthorizationRequest(
                AuthorizationOption.Alert |
                AuthorizationOption.Badge |
                AuthorizationOption.Sound,
                registerForRemoteNotifications: false
            );

            while (!request.IsFinished)
            {
                await System.Threading.Tasks.Task.Yield();
            }

            Debug.Log(
                $"iOS notification permission: " +
                $"Granted={request.Granted}, " +
                $"Error={request.Error}");
        }

        private string ScheduleIos(
            string title,
            string message,
            DateTime fireTime)
        {
            TimeSpan delay = fireTime - DateTime.Now;

            if (delay <= TimeSpan.Zero)
                return null;

            string id = Guid.NewGuid().ToString();

            var trigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = delay,
                Repeats = false
            };

            var notification = new iOSNotification
            {
                Identifier = id,
                Title = title,
                Body = message,

                ShowInForeground = true,

                ForegroundPresentationOption =
                    PresentationOption.Alert |
                    PresentationOption.Sound,

                Trigger = trigger
            };

            iOSNotificationCenter.ScheduleNotification(notification);

            return id;
        }

#endif
    }
}

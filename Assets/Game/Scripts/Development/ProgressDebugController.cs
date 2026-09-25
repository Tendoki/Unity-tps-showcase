using CodeBase.Data;
using System;
using Cysharp.Threading.Tasks;
using Firebase.Crashlytics;
using Game.Scripts.Services.Analytics;
using Game.Scripts.Services.Messaging;
using Game.Scripts.Services.PersistentProgress;
using Game.Scripts.Services.RemoteConfig;
using Game.Scripts.Services.SaveLoad;
using Game.Services;
using UnityEngine;

namespace Game.Scripts.Development
{
    public class ProgressDebugController : MonoBehaviour
    {
        [SerializeField] private string debugNickname = "Guest";

        private IPersistentProgressService _progressService;
        private ISaveLoadService _saveLoadService;
        private IAnalyticsService _analyticsService;
        private IRemoteConfigService _remoteConfigService;
        private INotificationService _notificationService;
        private bool _crashRequested;

        private void Awake()
        {
            _progressService = AllServices.Container.Single<IPersistentProgressService>();
            _saveLoadService = AllServices.Container.Single<ISaveLoadService>();
            _analyticsService = AllServices.Container.Single<IAnalyticsService>();
            _remoteConfigService = AllServices.Container.Single<IRemoteConfigService>();
            _notificationService = AllServices.Container.Single<INotificationService>();
        }

        public void AddCoins()
        {
            if (TryGetProgress(out PlayerProgress progress) == false)
                return;

            progress.Coins += _remoteConfigService.DebugBonusCoins;
            _analyticsService.LogCoinsChanged(progress.Coins);
            LogProgress("Coins changed");
        }

        public void LevelUp()
        {
            if (TryGetProgress(out PlayerProgress progress) == false)
                return;

            progress.Level++;
            _analyticsService.LogLevelChanged(progress.Level);
            LogProgress("Level changed");
        }

        public void ApplyNickname()
        {
            ApplyNicknameAsync().Forget();
        }

        public void Save()
        {
            SaveAsync().Forget();
        }

        public void LogCurrent()
        {
            LogProgress("Current progress");
        }

        public void TestLocalNotification()
        {
            DateTime fireTime = DateTime.Now.AddSeconds(5);

            string notificationId = _notificationService.Schedule(
                "Получено локальное уведомление",
                "У тебя построилось здание или типо того, что там было в Clash of clans",
                fireTime);

            Debug.Log($"Local notification scheduled. Id: {notificationId}, FireTime: {fireTime}");
        }

        public void Crash()
        {
            Debug.Log("Crashlytics test crash requested.");
            Crashlytics.ReportUncaughtExceptionsAsFatal = true;
            Crashlytics.Log("Crashlytics test crash requested from progress debug.");
            _crashRequested = true;
        }

        private void Update()
        {
            if (_crashRequested == false)
                return;

            _crashRequested = false;
            throw new Exception("Crashlytics test exception.");
        }

        private async UniTask SaveAsync()
        {
            if (TryGetProgress(out PlayerProgress progress) == false)
                return;

            await _saveLoadService.SaveProgress();
            LogProgress("Progress saved");
        }

        private async UniTask ApplyNicknameAsync()
        {
            await _saveLoadService.UpdateNickname(debugNickname);
            _analyticsService.LogNicknameChanged(_progressService.Progress.Nickname);
            LogProgress("Nickname changed");
        }

        private bool TryGetProgress(out PlayerProgress progress)
        {
            progress = _progressService.Progress;

            if (progress != null)
                return true;

            Debug.LogWarning("PlayerProgress is not loaded yet.");
            return false;
        }

        private void LogProgress(string message)
        {
            if (!TryGetProgress(out PlayerProgress progress))
                return;

            Debug.Log(
                $"{message}. Coins: {progress.Coins}, Level: {progress.Level}, Nickname: {progress.Nickname}");
        }
    }
}

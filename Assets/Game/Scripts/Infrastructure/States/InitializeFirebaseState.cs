using Cysharp.Threading.Tasks;
using Firebase;
using Game.Infrastructure.StateMachines;
using Game.Scripts.Services.Auth;
using Game.Scripts.Services.Messaging;
using Game.Scripts.Services.RemoteConfig;
using UnityEngine;

namespace Game.Scripts.Infrastructure.States
{
    public class InitializeFirebaseState : AsyncBaseState
    {
        private readonly IPlayerAuthService _playerAuthService;
        private readonly INotificationService _notificationService;
        private readonly IMessagingService _messagingService;
        private readonly IRemoteConfigService _remoteConfigService;

        public InitializeFirebaseState(IPlayerAuthService playerAuthService,
            INotificationService notificationService,
            IMessagingService messagingService,
            IRemoteConfigService remoteConfigService)
        {
            _playerAuthService = playerAuthService;
            _notificationService = notificationService;
            _messagingService = messagingService;
            _remoteConfigService = remoteConfigService;
        }

        public override async UniTask Enter()
        {
            _notificationService.Initialize();
            _notificationService.RequestPermission();

            bool firebaseReady = await CheckFirebaseDependenciesAsync();

            if (firebaseReady)
            {
                bool authInitialized = await _playerAuthService.InitializeAsync();

                if (authInitialized)
                {
                    bool signedIn = _playerAuthService.IsSignedIn || await _playerAuthService.SignInGuestAsync();

                    if (signedIn)
                    {
                        await _remoteConfigService.InitializeAsync();
                        await _messagingService.InitializeAsync();
                    }
                }
            }
            else
            {
                Debug.LogWarning("Firebase initialization failed. Continue with local progress.");
            }
        }

        private async UniTask<bool> CheckFirebaseDependenciesAsync()
        {
            DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();

            if (dependencyStatus == DependencyStatus.Available)
                return true;

            Debug.LogError($"Firebase dependencies are not available: {dependencyStatus}");
            return false;
        }
    }
}

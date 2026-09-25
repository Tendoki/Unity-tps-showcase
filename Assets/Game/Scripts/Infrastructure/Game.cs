using Game.Scripts.Infrastructure.States;
using Game.Infrastructure.StateMachines;
using Game.Scripts.Logic;
using Game.Services;
using Game.Scripts.Services.Analytics;
using Game.Scripts.Services.Auth;
using Game.Scripts.Services.Firebase;
using Game.Scripts.Services.Input;
using Game.Scripts.Services.Messaging;
using Game.Scripts.Services.PersistentProgress;
using Game.Scripts.Services.RemoteConfig;
using Game.Scripts.Services.SaveLoad;
using UnityEngine;

namespace Game.Infrastructure
{
    public class Game
    {
        public GameStateMachine StateMachine { get; }
        public PlayerDataBackend PlayerDataBackend { get; }

        public Game(LoadingCurtain loadingCurtain, PlayerDataBackend playerDataBackend)
        {
            PlayerDataBackend = playerDataBackend;
            Debug.Log($"Player data backend: {PlayerDataBackend}");

            AllServices services = AllServices.Container;
            RegisterServices(services, PlayerDataBackend);

            var sceneLoader = new SceneLoader();
            StateMachine = new GameStateMachine();
            var bootstrap = new BootstrapState(sceneLoader);
            AsyncBaseState initializeBackend = PlayerDataBackend == PlayerDataBackend.Firebase
                ? new InitializeFirebaseState(services.Single<IPlayerAuthService>(),
                    services.Single<INotificationService>(),
                    services.Single<IMessagingService>(),
                    services.Single<IRemoteConfigService>())
                : new InitializeRestState(services.Single<IPlayerAuthService>());
            var loadProgress = new LoadProgressState(services.Single<IPersistentProgressService>(),
                services.Single<ISaveLoadService>());
            var loadLevel = new LoadLevelState(sceneLoader, loadingCurtain, "Main");
            var gameLoop = new GameLoopState();

            StateMachine.AddTransition(bootstrap, initializeBackend);
            StateMachine.AddTransition(initializeBackend, loadProgress);
            StateMachine.AddTransition(loadProgress, loadLevel);
            StateMachine.AddTransition(loadLevel, gameLoop);

            StateMachine.SetState(bootstrap);
        }

        private static void RegisterServices(AllServices services, PlayerDataBackend playerDataBackend)
        {
            services.RegisterSingle<IInputService>(CreateInputService());
            services.RegisterSingle<IAnalyticsService>(new FirebaseAnalyticsService());
            services.RegisterSingle<IRemoteConfigService>(new FirebaseRemoteConfigService());
            services.RegisterSingle<INotificationService>(new LocalNotificationService());
            services.RegisterSingle<IMessagingService>(new FirebaseMessagingService());
            services.RegisterSingle<IPersistentProgressService>(new PersistentProgressService());

            if (playerDataBackend == PlayerDataBackend.Firebase)
            {
                services.RegisterSingle<IPlayerAuthService>(new FirebaseAuthService(
                    services.Single<IAnalyticsService>()));
                services.RegisterSingle<ISaveLoadService>(new FirestoreSaveLoadService(
                    services.Single<IPlayerAuthService>(),
                    services.Single<IPersistentProgressService>(),
                    services.Single<IAnalyticsService>()));
                return;
            }

            var restApiClient = new RestApiClient("http://localhost:5255");
            var restAuthService = new RestAuthService(restApiClient);
            var restPlayerApi = new RestPlayerApi(restAuthService, restApiClient);

            services.RegisterSingle<IPlayerAuthService>(restAuthService);
            services.RegisterSingle<ISaveLoadService>(new RestSaveLoadService(
                restPlayerApi,
                services.Single<IPersistentProgressService>()));
        }

        private static IInputService CreateInputService()
        {
            if (Application.isEditor)
                return new MobileInputService();

            return Application.isMobilePlatform
                ? new MobileInputService()
                : new StandaloneInputService();
        }
    }
}

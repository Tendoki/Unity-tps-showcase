using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using Game.Infrastructure.StateMachines;
using Game.Scripts.Logic;

namespace Game.Scripts.Infrastructure.States
{
    public class LoadLevelState : AsyncBaseState
    {
        private readonly string _sceneName;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _loadingCurtain;

        public LoadLevelState(SceneLoader sceneLoader,
            LoadingCurtain loadingCurtain, string sceneName)
        {
            _sceneName = sceneName;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
        }

        public override async UniTask Enter()
        {
            _loadingCurtain?.Show();
            await _sceneLoader.Load(_sceneName);
        }

        public override UniTask Exit()
        {
            _loadingCurtain?.Hide();
            return UniTask.CompletedTask;
        }
    }
}

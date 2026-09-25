using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using Game.Infrastructure.StateMachines;

namespace Game.Scripts.Infrastructure.States
{
    public class BootstrapState : AsyncBaseState
    {
        private const string Initial = "Initial";

        private readonly SceneLoader _sceneLoader;

        public BootstrapState(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public override async UniTask Enter()
        {
            await _sceneLoader.Load(Initial);
        }
    }
}

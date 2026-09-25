using Cysharp.Threading.Tasks;
using Game.Infrastructure.StateMachines;
using Game.Scripts.Services.Auth;

namespace Game.Scripts.Infrastructure.States
{
    public class InitializeRestState : AsyncBaseState
    {
        private readonly IPlayerAuthService _playerAuthService;

        public InitializeRestState(IPlayerAuthService playerAuthService)
        {
            _playerAuthService = playerAuthService;
        }

        public override async UniTask Enter()
        {
            await _playerAuthService.InitializeAsync();
            await _playerAuthService.SignInGuestAsync();
        }
    }
}

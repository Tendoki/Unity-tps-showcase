using Cysharp.Threading.Tasks;
using Game.Services;

namespace Game.Scripts.Services.Auth
{
    public interface IPlayerAuthService : IService
    {
        bool IsInitialized { get; }
        bool IsSignedIn { get; }
        string UserId { get; }

        UniTask<bool> InitializeAsync();
        UniTask<bool> SignInGuestAsync();
    }
}

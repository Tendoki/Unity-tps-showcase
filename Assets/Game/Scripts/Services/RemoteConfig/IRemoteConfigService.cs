using Cysharp.Threading.Tasks;
using Game.Services;

namespace Game.Scripts.Services.RemoteConfig
{
    public interface IRemoteConfigService : IService
    {
        int DebugBonusCoins { get; }

        UniTask InitializeAsync();
    }
}

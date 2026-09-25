using Cysharp.Threading.Tasks;
using Game.Services;

namespace Game.Scripts.Services.Messaging
{
    public interface IMessagingService : IService
    {
        string Token { get; }

        UniTask InitializeAsync();
    }
}

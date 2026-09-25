using Cysharp.Threading.Tasks;

namespace Game.Infrastructure.StateMachines
{
    public interface IAsyncState
    {
        UniTask Enter();
        UniTask Exit();
    }
}

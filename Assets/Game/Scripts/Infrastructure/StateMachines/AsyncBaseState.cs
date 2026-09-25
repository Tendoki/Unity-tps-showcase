using Cysharp.Threading.Tasks;

namespace Game.Infrastructure.StateMachines
{
    public abstract class AsyncBaseState : IAsyncState
    {
        public virtual UniTask Enter() => UniTask.CompletedTask;

        public virtual UniTask Exit() => UniTask.CompletedTask;
    }
}

using Game.FSM;

namespace Game.Infrastructure.StateMachines
{
    public class Transition
    {
        public IAsyncState To { get; }
        public IPredicate Condition { get; }

        public Transition(IAsyncState to, IPredicate condition = null)
        {
            To = to;
            Condition = condition;
        }
    }
}

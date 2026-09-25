using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.FSM;

namespace Game.Infrastructure.StateMachines
{
    public class AsyncStateMachine
    {
        private readonly Dictionary<IAsyncState, StateNode> _nodes = new();
        private StateNode _current;
        private bool _entered;

        public IAsyncState CurrentState => _current?.State;
        public bool IsTransitioning { get; private set; }

        public void Update()
        {
            if (IsTransitioning || !_entered)
                return;

            Transition transition = GetTransition();
            if (transition != null)
                SetState(transition.To);
        }

        public void SetState(IAsyncState state)
        {
            if (IsTransitioning)
                throw new InvalidOperationException("A state transition is already in progress.");

            StateNode next = GetOrAddNode(state);
            if (next == _current)
                return;

            IsTransitioning = true;
            _entered = false;
            ChangeState(next).Forget();
        }

        public void AddTransition(IAsyncState from, IAsyncState to, IPredicate condition = null)
        {
            StateNode source = GetOrAddNode(from);
            StateNode target = GetOrAddNode(to);
            source.Transitions.Add(new Transition(target.State, condition));
        }

        private async UniTask ChangeState(StateNode next)
        {
            try
            {
                if (_current != null)
                    await _current.State.Exit();

                _current = next;
                await _current.State.Enter();
                _entered = true;
            }
            finally
            {
                IsTransitioning = false;
            }
        }

        private Transition GetTransition()
        {
            foreach (Transition transition in _current.Transitions)
            {
                if (ReferenceEquals(transition.To, _current.State))
                    continue;

                if (transition.Condition == null || transition.Condition.Evaluate())
                    return transition;
            }

            return null;
        }

        private StateNode GetOrAddNode(IAsyncState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (!_nodes.TryGetValue(state, out StateNode node))
            {
                node = new StateNode(state);
                _nodes.Add(state, node);
            }

            return node;
        }

        private class StateNode
        {
            public IAsyncState State { get; }
            public List<Transition> Transitions { get; } = new();

            public StateNode(IAsyncState state)
            {
                State = state;
            }
        }
    }
}

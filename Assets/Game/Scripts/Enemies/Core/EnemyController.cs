using Game.Enemies.States;
using Game.Enemies.Components;
using System;
using Game.FSM;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Enemies
{
    public class EnemyController : MonoBehaviour, IEnemyStateController
    {
        [Header("References")]
        [SerializeField] private EnemyTargetDetector targetDetector;
        [SerializeField] private EnemyNavigation navigation;
        [FormerlySerializedAs("mover")]
        [SerializeField] private EnemyLocomotionController locomotion;
        [SerializeField] private EnemyAnimationController animationController;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;

        private StateMachine _stateMachine;
        private float _health;
        private bool _wasHit;

        protected StateMachine StateMachine => _stateMachine;

        public Transform EnemyTransform => transform;
        public EnemyTargetDetector TargetDetector => targetDetector;
        public EnemyNavigation Navigation => navigation;
        public EnemyLocomotionController Locomotion => locomotion;
        public EnemyAnimationController Animation => animationController;
        public bool IsDead => _health <= 0f;
        public bool WasHit => _wasHit;
        public IState CurrentState => _stateMachine.CurrentState;

        protected virtual void Awake()
        {
            _health = maxHealth;
            _stateMachine = new StateMachine();
            SetupStateMachine();
        }

        protected virtual void Update()
        {
            _stateMachine.Update();
        }

        protected virtual void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
            navigation.SyncAgentPosition(transform.position);
        }

        public void TakeDamage(float damage)
        {
            if (IsDead)
            {
                return;
            }

            _health = Mathf.Max(0f, _health - damage);
            _wasHit = true;
        }

        protected virtual void SetupStateMachine()
        {
            var idle = new EnemyIdleState(this);
            var hitReaction = new EnemyHitReactionState(this);
            var dead = new EnemyDeadState(this);

            At(hitReaction, idle, () => animationController.IsHitReactionFinished && !IsDead);

            Any(hitReaction, () => WasHit && !IsDead);
            Any(dead, () => IsDead);

            _stateMachine.SetState(idle);
        }

        protected void At(IState from, IState to, Func<bool> condition)
        {
            _stateMachine.AddTransition(from, to, new FuncPredicate(condition));
        }

        protected void Any(IState to, Func<bool> condition)
        {
            _stateMachine.AddAnyTransition(to, new FuncPredicate(condition));
        }

        public void ConsumeHit()
        {
            _wasHit = false;
        }
    }
}

using Game.Enemies.Zombie.States;
using Game.Enemies.States;
using Game.Enemies;
using Game.FSM;
using UnityEngine;

namespace Game.Enemies.Zombie
{
    public class ZombieController : EnemyController, IZombieStateController
    {
        [Header("Patrol")]
        [SerializeField] private float patrolSpeed = 1.5f;
        [SerializeField] private float patrolRadius = 8f;
        [SerializeField] private float patrolStoppingDistance = 0.35f;
        [SerializeField] private float minPatrolWaitTime = 1f;
        [SerializeField] private float maxPatrolWaitTime = 3f;

        [Header("Chase")]
        [SerializeField] private float chaseSpeed = 3.5f;
        [SerializeField] private float chaseDestinationSearchRadius = 6f;
        [SerializeField] private int chaseDestinationSampleCount = 16;

        [Header("Ground Probe")]
        [Range(0f, 1f)]
        [SerializeField] private float patrolGroundProbeOffsetRatio = 0.45f;
        [Range(0f, 1f)]
        [SerializeField] private float chaseGroundProbeOffsetRatio = 0.65f;

        [Header("Attack")]
        [SerializeField] private float attackDistance = 1.5f;
        [SerializeField] private float attackCooldown = 1.2f;

        [Header("Rotation")]
        [SerializeField] private float movementRotationSmoothing = 10f;
        [SerializeField] private float attackRotationSmoothing = 25f;

        [Header("Animation")]
        [SerializeField] private bool useRootMotionLocomotion;

        private float _nextAttackTime;
        private Vector3 _lastKnownTargetPosition;
        private bool _hasLastKnownTargetPosition;

        public float PatrolSpeed => patrolSpeed;
        public float PatrolRadius => patrolRadius;
        public float PatrolStoppingDistance => patrolStoppingDistance;
        public float MinPatrolWaitTime => minPatrolWaitTime;
        public float MaxPatrolWaitTime => maxPatrolWaitTime;
        public float ChaseSpeed => chaseSpeed;
        public float ChaseDestinationSearchRadius => chaseDestinationSearchRadius;
        public int ChaseDestinationSampleCount => chaseDestinationSampleCount;
        public float PatrolGroundProbeOffsetRatio => patrolGroundProbeOffsetRatio;
        public float ChaseGroundProbeOffsetRatio => chaseGroundProbeOffsetRatio;
        public float AttackDistance => attackDistance;
        public float AttackCooldown => attackCooldown;
        public float MovementRotationSmoothing => movementRotationSmoothing;
        public float AttackRotationSmoothing => attackRotationSmoothing;
        public bool UseRootMotionLocomotion => useRootMotionLocomotion;
        public Vector3 PatrolCenter { get; private set; }
        public Vector3 LastKnownTargetPosition => _lastKnownTargetPosition;
        public bool IsAttackReady => Time.time >= _nextAttackTime;
        public bool HasLastKnownTargetPosition => _hasLastKnownTargetPosition;

        protected override void Awake()
        {
            PatrolCenter = transform.position;
            base.Awake();
        }

        private void Start()
        {
            Animation.SetRootMotionEnabled(useRootMotionLocomotion);
        }

        protected override void SetupStateMachine()
        {
            var patrol = new ZombiePatrolState(this);
            var chase = new ZombieChaseState(this);
            var investigate = new ZombieInvestigateState(this);
            var attack = new ZombieAttackState(this);
            var hitReaction = new EnemyHitReactionState(this);
            var dead = new EnemyDeadState(this);

            At(patrol, chase, HasDetectedTarget);
            At(chase, attack, CanAttackTarget);
            At(chase, investigate, HasLostTarget);
            At(investigate, chase, HasDetectedTarget);
            At(investigate, patrol, () => investigate.IsFinished);
            At(attack, chase, ShouldLeaveAttack);
            At(hitReaction, chase, () => Animation.IsHitReactionFinished && !IsDead && TargetDetector.HasTarget);
            At(hitReaction, investigate, () => Animation.IsHitReactionFinished && !IsDead && !TargetDetector.HasTarget && HasLastKnownTargetPosition);
            At(hitReaction, patrol, () => Animation.IsHitReactionFinished && !IsDead && !TargetDetector.HasTarget && !HasLastKnownTargetPosition);

            Any(hitReaction, () => WasHit && !IsDead);
            Any(dead, () => IsDead);

            StateMachine.SetState(patrol);
        }

        public void StartAttackCooldown()
        {
            _nextAttackTime = Time.time + attackCooldown;
        }

        public void RememberTargetPosition(Vector3 position)
        {
            _lastKnownTargetPosition = position;
            _hasLastKnownTargetPosition = true;
        }

        public void ClearLastKnownTargetPosition()
        {
            _hasLastKnownTargetPosition = false;
        }

        private bool HasDetectedTarget()
        {
            return TargetDetector.RefreshTarget();
        }

        private bool CanAttackTarget()
        {
            return TargetDetector.HasTarget
                   && TargetDetector.DistanceToTarget <= attackDistance
                   && IsAttackReady;
        }

        private bool HasLostTarget()
        {
            return !TargetDetector.RefreshTarget();
        }

        private bool ShouldLeaveAttack()
        {
            return Animation.IsAttackFinished
                   && (!TargetDetector.RefreshTarget() || TargetDetector.DistanceToTarget > attackDistance);
        }
    }
}

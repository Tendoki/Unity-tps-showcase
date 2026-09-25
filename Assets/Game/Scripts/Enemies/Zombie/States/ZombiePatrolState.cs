using Game.Enemies.Zombie;
using Game.Enemies.States;
using Game.Enemies;
using UnityEngine;

namespace Game.Enemies.Zombie.States
{
    public class ZombiePatrolState : EnemyState
    {
        private enum PatrolMode
        {
            Moving,
            Waiting
        }

        private readonly IZombieStateController _zombie;
        private float _waitTimer;
        private PatrolMode _mode;

        public ZombiePatrolState(IZombieStateController zombie) : base(zombie)
        {
            _zombie = zombie;
        }

        public override void OnEnter()
        {
            _zombie.Locomotion.ResetRootMotionRotation(_zombie.EnemyTransform.rotation);
            StartWaiting();
        }

        public override void Update()
        {
            _zombie.TargetDetector.RefreshTarget();

            switch (_mode)
            {
                case PatrolMode.Moving:
                    break;
                case PatrolMode.Waiting:
                    UpdateWaiting();
                    break;
            }
        }

        public override void FixedUpdate()
        {
            switch (_mode)
            {
                case PatrolMode.Moving:
                    FixedUpdateMoving();
                    break;
                case PatrolMode.Waiting:
                    _zombie.Animation.DiscardRootMotion();
                    _zombie.Locomotion.Stop();
                    break;
            }
        }

        private void UpdateWaiting()
        {
            _waitTimer -= Time.deltaTime;

            if (_waitTimer <= 0f)
            {
                SelectPatrolDestination();
            }
        }

        private void FixedUpdateMoving()
        {
            if (_zombie.Navigation.HasReachedDestination(
                    _zombie.PatrolStoppingDistance))
            {
                StartWaiting();
                return;
            }

            Vector3 moveDirection = Vector3.ProjectOnPlane(
                _zombie.Navigation.DesiredVelocity,
                Vector3.up
            );

            if (_zombie.UseRootMotionLocomotion)
            {
                if (_zombie.Animation.ConsumeRootMotion(
                        out Vector3 rootMotionDelta,
                        out Quaternion rootMotionRotation,
                        out float rootMotionDeltaTime))
                {
                    _zombie.Locomotion.MoveRootMotion(
                        moveDirection,
                        rootMotionDelta,
                        rootMotionDeltaTime,
                        _zombie.PatrolGroundProbeOffsetRatio,
                        Time.fixedDeltaTime
                    );

                    _zombie.Locomotion.RotateRootMotion(
                        moveDirection,
                        rootMotionRotation,
                        _zombie.MovementRotationSmoothing,
                        Time.fixedDeltaTime
                    );
                }

                return;
            }

            _zombie.Animation.DiscardRootMotion();

            _zombie.Locomotion.Move(
                moveDirection,
                _zombie.PatrolSpeed,
                _zombie.PatrolGroundProbeOffsetRatio,
                Time.fixedDeltaTime
            );

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

                _zombie.Locomotion.Rotate(
                    targetRotation,
                    _zombie.MovementRotationSmoothing,
                    Time.fixedDeltaTime
                );
            }
        }

        private void SelectPatrolDestination()
        {
            const int maxAttempts = 8;

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 randomPoint = Random.insideUnitCircle * _zombie.PatrolRadius;
                Vector3 destination = _zombie.PatrolCenter + new Vector3(randomPoint.x, 0f, randomPoint.y);

                if (_zombie.Navigation.TrySamplePosition(destination, out Vector3 sampledPosition)
                    && _zombie.Navigation.CanReach(sampledPosition))
                {
                    _zombie.Navigation.SetDestination(sampledPosition);
                    _mode = PatrolMode.Moving;
                    _zombie.Animation.PlayWalk();
                    return;
                }
            }

            StartWaiting();
        }

        private void StartWaiting()
        {
            _mode = PatrolMode.Waiting;
            _waitTimer = Random.Range(_zombie.MinPatrolWaitTime, _zombie.MaxPatrolWaitTime);
            _zombie.Locomotion.ResetRootMotionRotation(_zombie.EnemyTransform.rotation);
            _zombie.Locomotion.Stop();
            _zombie.Animation.PlayIdle();
        }
    }
}

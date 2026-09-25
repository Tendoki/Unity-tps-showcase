using Game.Enemies.Zombie;
using Game.Enemies.States;
using Game.Enemies;
using UnityEngine;

namespace Game.Enemies.Zombie.States
{
    public class ZombieChaseState : EnemyState
    {
        private readonly IZombieStateController _zombie;

        public ZombieChaseState(IZombieStateController zombie) : base(zombie)
        {
            _zombie = zombie;
        }

        public override void OnEnter()
        {
            _zombie.Locomotion.ResetRootMotionRotation(_zombie.EnemyTransform.rotation);
            _zombie.Animation.PlayRun();
        }
        
        public override void FixedUpdate()
        {
            if (!_zombie.TargetDetector.RefreshTarget())
            {
                _zombie.Animation.DiscardRootMotion();
                _zombie.Locomotion.ResetRootMotionRotation(_zombie.EnemyTransform.rotation);
                _zombie.Locomotion.Stop();
                return;
            }

            Transform target = _zombie.TargetDetector.CurrentTarget;

            if (!_zombie.Navigation.TryFindReachablePointNear(
                    target.position,
                    _zombie.ChaseDestinationSearchRadius,
                    _zombie.ChaseDestinationSampleCount,
                    out Vector3 destination)
                || !_zombie.Navigation.SetDestination(destination))
            {
                _zombie.Animation.DiscardRootMotion();
                _zombie.Locomotion.ResetRootMotionRotation(_zombie.EnemyTransform.rotation);
                _zombie.Locomotion.Stop();
                return;
            }

            _zombie.RememberTargetPosition(destination);

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
                        _zombie.ChaseGroundProbeOffsetRatio,
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
                _zombie.ChaseSpeed,
                _zombie.ChaseGroundProbeOffsetRatio,
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
    }
}

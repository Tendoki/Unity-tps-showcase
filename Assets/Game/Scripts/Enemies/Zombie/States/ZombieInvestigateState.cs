using Game.Enemies.Zombie;
using Game.Enemies.States;
using Game.Enemies;
using UnityEngine;

namespace Game.Enemies.Zombie.States
{
    public class ZombieInvestigateState : EnemyState
    {
        private readonly IZombieStateController _zombie;

        public bool IsFinished { get; private set; }

        public ZombieInvestigateState(IZombieStateController zombie) : base(zombie)
        {
            _zombie = zombie;
        }

        public override void OnEnter()
        {
            IsFinished = false;
            _zombie.Locomotion.ResetRootMotionRotation(_zombie.EnemyTransform.rotation);

            if (!_zombie.HasLastKnownTargetPosition
                || !_zombie.Navigation.SetDestination(_zombie.LastKnownTargetPosition))
            {
                Finish();
                return;
            }

            _zombie.Animation.PlayRun();
        }

        public override void Update()
        {
            _zombie.TargetDetector.RefreshTarget();
        }

        public override void FixedUpdate()
        {
            if (IsFinished)
            {
                _zombie.Animation.DiscardRootMotion();
                _zombie.Locomotion.Stop();
                return;
            }

            if (_zombie.Navigation.HasReachedDestination(
                    _zombie.PatrolStoppingDistance))
            {
                Finish();
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

        private void Finish()
        {
            IsFinished = true;
            _zombie.ClearLastKnownTargetPosition();
            _zombie.Animation.DiscardRootMotion();
            _zombie.Locomotion.ResetRootMotionRotation(_zombie.EnemyTransform.rotation);
            _zombie.Locomotion.Stop();
        }
    }
}

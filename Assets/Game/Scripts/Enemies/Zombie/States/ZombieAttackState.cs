using Game.Enemies.Zombie;
using Game.Enemies.States;
using Game.Enemies;
using UnityEngine;

namespace Game.Enemies.Zombie.States
{
    public class ZombieAttackState : EnemyState
    {
        private enum AttackMode
        {
            Attacking,
            Cooldown
        }

        private readonly IZombieStateController _zombie;
        private AttackMode _mode;

        public ZombieAttackState(IZombieStateController zombie) : base(zombie)
        {
            _zombie = zombie;
        }

        public override void OnEnter()
        {
            StartAttack();
        }

        public override void Update()
        {
            if (!_zombie.TargetDetector.RefreshTarget())
            {
                return;
            }

            Transform target = _zombie.TargetDetector.CurrentTarget;
            Vector3 lookDirection = Vector3.ProjectOnPlane(
                target.position - _zombie.EnemyTransform.position,
                Vector3.up);

            if (lookDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                _zombie.Locomotion.Rotate(targetRotation, _zombie.AttackRotationSmoothing, Time.deltaTime);
            }

            switch (_mode)
            {
                case AttackMode.Attacking:
                    UpdateAttacking();
                    break;
                case AttackMode.Cooldown:
                    UpdateCooldown();
                    break;
            }
        }

        public override void FixedUpdate()
        {
            _zombie.Animation.DiscardRootMotion();
            _zombie.Locomotion.Stop();
        }

        private void UpdateAttacking()
        {
            if (!_zombie.Animation.IsAttackFinished)
            {
                return;
            }

            _zombie.StartAttackCooldown();
            _mode = AttackMode.Cooldown;
        }

        private void UpdateCooldown()
        {
            if (_zombie.TargetDetector.DistanceToTarget > _zombie.AttackDistance || !_zombie.IsAttackReady)
            {
                return;
            }

            StartAttack();
        }

        private void StartAttack()
        {
            _mode = AttackMode.Attacking;
            _zombie.Animation.PlayAttack();
        }
    }
}

using System;
using UnityEngine;

namespace Game.Enemies.Components
{
    public class EnemyAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private bool hasHitReactionAnimation;
        [SerializeField] private float locomotionFadeTime = 0.15f;
        [SerializeField] private float actionFadeTime = 0.05f;

        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int WalkHash = Animator.StringToHash("Walk");
        private static readonly int RunHash = Animator.StringToHash("Run");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeadHash = Animator.StringToHash("Dead");

        private Vector3 _rootMotionDeltaPosition;
        private float _rootMotionDeltaTime;
        private Quaternion _rootMotionDeltaRotation = Quaternion.identity;

        public event Action AttackHit;

        public bool IsAttackFinished { get; private set; } = true;
        public bool IsHitReactionFinished { get; private set; } = true;

        private void Awake()
        {
            animator.applyRootMotion = false;
        }

        private void OnAnimatorMove()
        {
            _rootMotionDeltaPosition += animator.deltaPosition;
            _rootMotionDeltaRotation *= animator.deltaRotation;
            _rootMotionDeltaTime += Time.deltaTime;
        }

        public void SetRootMotionEnabled(bool isEnabled)
        {
            animator.applyRootMotion = isEnabled;

            if (!isEnabled)
            {
                DiscardRootMotion();
            }
        }

        public bool ConsumeRootMotion(
            out Vector3 deltaPosition,
            out Quaternion deltaRotation,
            out float deltaTime)
        {
            deltaPosition = _rootMotionDeltaPosition;
            deltaRotation = _rootMotionDeltaRotation;
            deltaTime = _rootMotionDeltaTime;

            _rootMotionDeltaPosition = Vector3.zero;
            _rootMotionDeltaRotation = Quaternion.identity;
            _rootMotionDeltaTime = 0f;

            return deltaTime > 0f;
        }

        public void DiscardRootMotion()
        {
            _rootMotionDeltaPosition = Vector3.zero;
            _rootMotionDeltaRotation = Quaternion.identity;
            _rootMotionDeltaTime = 0f;
        }

        public void PlayIdle()
        {
            animator.CrossFade(IdleHash, locomotionFadeTime);
        }

        public void PlayWalk()
        {
            animator.CrossFade(WalkHash, locomotionFadeTime);
        }

        public void PlayRun()
        {
            animator.CrossFade(RunHash, locomotionFadeTime);
        }

        public void PlayAttack()
        {
            IsAttackFinished = false;
            animator.CrossFade(AttackHash, actionFadeTime, 0, 0f);
        }

        public void PlayHitReaction()
        {
            if (!hasHitReactionAnimation)
            {
                IsHitReactionFinished = true;
                return;
            }

            IsHitReactionFinished = false;
            animator.CrossFade(HitHash, actionFadeTime);
        }

        public void PlayDeath()
        {
            animator.CrossFade(DeadHash, actionFadeTime);
        }

        public void OnAttackHit()
        {
            AttackHit?.Invoke();
        }

        public void OnAttackFinished()
        {
            IsAttackFinished = true;
        }

        public void OnHitReactionFinished()
        {
            IsHitReactionFinished = true;
        }
    }
}

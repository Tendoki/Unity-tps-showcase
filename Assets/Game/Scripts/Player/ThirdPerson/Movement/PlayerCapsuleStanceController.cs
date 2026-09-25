using Game.Actors.Movement;
using UnityEngine;

namespace Game.Player.ThirdPerson.Movement
{
    public class PlayerCapsuleStanceController : ActorCapsuleStanceController
    {
        [Header("Crouch")]
        [SerializeField] private float crouchingHeight = 1f;
        [SerializeField] private LayerMask standCheckMask;

        [Header("Timing")]
        [SerializeField] private float crouchTransitionSpeed = 8f;

        private bool _usingCrouchCollider;
        private float _standingHeight;
        private Vector3 _standingCenter;
        private float _standingRadius;
        private Vector3 _crouchingCenter;
        private readonly Collider[] _hits = new Collider[32];

        public float StandingLerp { get; private set; } = 1f;

        protected override void Awake()
        {
            base.Awake();

            _standingHeight = CapsuleCollider.height;
            _standingCenter = CapsuleCollider.center;
            _standingRadius = CapsuleCollider.radius;
            _crouchingCenter = GetCrouchingCenter();
            ApplyStandingCollider();
        }

        public void Tick(bool isCrouching, float deltaTime)
        {
            float targetLerp = isCrouching ? 0f : 1f;
            StandingLerp = Mathf.MoveTowards(StandingLerp, targetLerp, crouchTransitionSpeed * deltaTime);

            UpdateColliderShape();
        }

        public bool CanStand()
        {
            GetStandingCapsuleWorldPoints(out Vector3 bottom, out Vector3 top, out float radius);
            int hitCount = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, _hits, standCheckMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                if (_hits.Length > i && _hits[i] != CapsuleCollider)
                {
                    return false;
                }
            }

            return true;
        }

        private void GetStandingCapsuleWorldPoints(out Vector3 bottom, out Vector3 top, out float radius)
        {
            GetCapsuleWorldPoints(
                _standingHeight,
                _standingCenter,
                _standingRadius,
                out bottom,
                out top,
                out radius);
        }

        private void UpdateColliderShape()
        {
            bool shouldUseCrouchCollider = StandingLerp <= 0.5f;

            if (shouldUseCrouchCollider == _usingCrouchCollider)
            {
                return;
            }

            if (shouldUseCrouchCollider)
            {
                ApplyCrouchingCollider();
            }
            else
            {
                ApplyStandingCollider();
            }

            _usingCrouchCollider = shouldUseCrouchCollider;
        }

        private void ApplyStandingCollider()
        {
            CapsuleCollider.height = _standingHeight;
            CapsuleCollider.center = _standingCenter;
            CapsuleCollider.radius = _standingRadius;
        }

        private void ApplyCrouchingCollider()
        {
            CapsuleCollider.height = crouchingHeight;
            CapsuleCollider.center = _crouchingCenter;
        }

        private Vector3 GetCrouchingCenter()
        {
            Vector3 center = _standingCenter;
            float standingBottom = _standingCenter.y - _standingHeight * 0.5f;
            center.y = standingBottom + crouchingHeight * 0.5f;
            return center;
        }

        private void OnDrawGizmosSelected()
        {
            if (StandingLerp >= 1f)
            {
                return;
            }

            GetStandingCapsuleWorldPoints(out Vector3 bottom, out Vector3 top, out float radius);

            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawLine(bottom + CapsuleCollider.transform.right * radius, top + CapsuleCollider.transform.right * radius);
            Gizmos.DrawLine(bottom - CapsuleCollider.transform.right * radius, top - CapsuleCollider.transform.right * radius);
            Gizmos.DrawLine(bottom + CapsuleCollider.transform.forward * radius, top + CapsuleCollider.transform.forward * radius);
            Gizmos.DrawLine(bottom - CapsuleCollider.transform.forward * radius, top - CapsuleCollider.transform.forward * radius);
        }
    }
}

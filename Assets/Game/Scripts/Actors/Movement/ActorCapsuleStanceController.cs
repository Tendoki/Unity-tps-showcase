using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Actors.Movement
{
    public class ActorCapsuleStanceController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private ActorGroundDetector groundDetector;

        [Header("Collider")]
        [FormerlySerializedAs("characterHeight")]
        [SerializeField] private float actorHeight = 2f;

        public CapsuleCollider CapsuleCollider => capsuleCollider;
        public float ActorHeight => actorHeight;
        public float MaxStepHeight => groundDetector != null ? groundDetector.MaxStepHeight : 0f;

        protected virtual void OnValidate()
        {
            ApplyConfiguredStandingCollider();
        }

        protected virtual void Awake()
        {
            ApplyConfiguredStandingCollider();
        }

        protected void ApplyConfiguredStandingCollider()
        {
            if (capsuleCollider == null)
            {
                return;
            }

            float minHeight = capsuleCollider.radius * 2f;
            float maxStepHeight = MaxStepHeight;
            float standingHeight = Mathf.Max(minHeight, actorHeight - maxStepHeight);
            Vector3 localAxis = GetLocalCapsuleAxis();
            Vector3 center = Vector3.ProjectOnPlane(capsuleCollider.center, localAxis);

            center += localAxis * (maxStepHeight + standingHeight * 0.5f);

            capsuleCollider.height = standingHeight;
            capsuleCollider.center = center;
        }

        protected void GetCapsuleWorldPoints(
            float height,
            Vector3 center,
            float radius,
            out Vector3 bottom,
            out Vector3 top,
            out float worldRadius)
        {
            transform.GetWorldPoints(height, center, radius, out bottom, out top, out worldRadius);
        }

        protected Vector3 GetLocalCapsuleAxis()
        {
            return capsuleCollider.direction switch
            {
                0 => Vector3.right,
                1 => Vector3.up,
                2 => Vector3.forward,
                _ => Vector3.up
            };
        }
    }
}

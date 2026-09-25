using UnityEngine;

namespace Game.Enemies.Components
{
    public class EnemySphereTargetDetector : EnemyTargetDetector
    {
        [SerializeField] private Transform eyePoint;
        [SerializeField] private float detectionDistance = 12f;
        [SerializeField] private float viewAngle = 120f;
        [SerializeField] private float verticalViewAngle = 90f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask obstacleMask;

        private readonly Collider[] _results = new Collider[8];
        private Transform _currentTarget;
        private Collider _currentTargetCollider;

        public override Transform CurrentTarget => _currentTarget;
        public override bool HasTarget => CurrentTarget != null;
        public override float DistanceToTarget => HasTarget
            ? Vector3.Distance(transform.position, CurrentTarget.position)
            : float.PositiveInfinity;
        public override bool HasLineOfSightToTarget => HasTarget && HasLineOfSight(CurrentTarget);

        public override bool TryDetectTarget()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                detectionDistance,
                _results,
                targetMask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider candidate = _results[i];

                if (IsInsideViewAngle(candidate) && HasLineOfSight(candidate))
                {
                    _currentTarget = candidate.transform;
                    _currentTargetCollider = candidate;
                    return true;
                }
            }

            return false;
        }

        public override bool RefreshTarget()
        {
            if (!HasTarget)
            {
                return TryDetectTarget();
            }

            if (DistanceToTarget <= detectionDistance && HasLineOfSightToTarget)
            {
                return true;
            }

            ClearTarget();
            return false;
        }

        public override void ClearTarget()
        {
            _currentTarget = null;
            _currentTargetCollider = null;
        }

        private bool HasLineOfSight(Transform target)
        {
            if (_currentTargetCollider != null && target == _currentTargetCollider.transform)
            {
                return HasLineOfSight(_currentTargetCollider);
            }

            return !Physics.Linecast(
                eyePoint.position,
                target.position,
                obstacleMask,
                QueryTriggerInteraction.Ignore);
        }

        private bool HasLineOfSight(Collider target)
        {
            Vector3 origin = eyePoint.position;
            Vector3 destination = target.bounds.center;

            return !Physics.Linecast(origin, destination, obstacleMask, QueryTriggerInteraction.Ignore);
        }

        private bool IsInsideViewAngle(Collider target)
        {
            Vector3 directionToTarget = target.bounds.center - eyePoint.position;
            Vector3 horizontalDirectionToTarget = Vector3.ProjectOnPlane(directionToTarget, Vector3.up);

            if (horizontalDirectionToTarget.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            Vector3 horizontalForward = Vector3.ProjectOnPlane(eyePoint.forward, Vector3.up);

            if (horizontalForward.sqrMagnitude <= 0.0001f)
            {
                horizontalForward = transform.forward;
            }

            float horizontalAngleToTarget = Vector3.Angle(horizontalForward, horizontalDirectionToTarget);

            if (horizontalAngleToTarget > viewAngle * 0.5f)
            {
                return false;
            }

            float verticalAngleToTarget = Vector3.Angle(directionToTarget, horizontalDirectionToTarget);
            return verticalAngleToTarget <= verticalViewAngle * 0.5f;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionDistance);

            Vector3 origin = eyePoint != null ? eyePoint.position : transform.position;
            Vector3 forward = eyePoint != null ? eyePoint.forward : transform.forward;
            Vector3 horizontalForward = Vector3.ProjectOnPlane(forward, Vector3.up);

            if (horizontalForward.sqrMagnitude <= 0.0001f)
            {
                horizontalForward = transform.forward;
            }

            horizontalForward.Normalize();
            Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle * 0.5f, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle * 0.5f, Vector3.up);

            Gizmos.color = Color.crimson;
            Gizmos.DrawRay(origin, leftRayRotation * horizontalForward * detectionDistance);
            Gizmos.DrawRay(origin, rightRayRotation * horizontalForward * detectionDistance);

            Vector3 verticalAxis = Vector3.Cross(horizontalForward, Vector3.up);
            Quaternion upperRayRotation = Quaternion.AngleAxis(-verticalViewAngle * 0.5f, verticalAxis);
            Quaternion lowerRayRotation = Quaternion.AngleAxis(verticalViewAngle * 0.5f, verticalAxis);

            Gizmos.color = Color.crimson;
            Gizmos.DrawRay(origin, upperRayRotation * horizontalForward * detectionDistance);
            Gizmos.DrawRay(origin, lowerRayRotation * horizontalForward * detectionDistance);
        }
    }
}

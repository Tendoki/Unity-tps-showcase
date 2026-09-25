using UnityEngine;

namespace Game.Actors.Movement
{
    public class ActorGroundDetector : MonoBehaviour
    {
        [Header("Ground Check")]
        [SerializeField] private ActorCapsuleStanceController capsuleStance;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float maxSlopeAngle = 50f;
        [Range(0f, 1f)]
        [SerializeField] private float stepHeightRatio = 0.175f;
        [SerializeField] private float minForwardStepUpHeight = 0.005f;
        [SerializeField] private float minGroundNormalDot = 0.2f;
        [SerializeField] private float groundProbeOffset = 0.02f;

        private Vector3 _groundNormal = Vector3.up;
        private Vector3 _groundPoint;
        private Vector3 _probeOrigin;
        private Vector3 _probeDirection = Vector3.down;
        private float _groundDistance;
        private float _probeDistance;
        private readonly Vector3[] _sampleOrigins = new Vector3[2];
        private readonly bool[] _sampleHits = new bool[2];
        private readonly bool[] _sampleValid = new bool[2];
        private int _sampleCount;
        private int _selectedSampleIndex = -1;

        public bool IsGrounded { get; private set; }
        public bool HasGroundHit { get; private set; }
        public bool IsTooSteep { get; private set; }
        public Vector3 GroundNormal => _groundNormal;
        public Vector3 GroundPoint => _groundPoint;
        public Vector3 ProbeOrigin => _probeOrigin;
        public Vector3 ProbeDirection => _probeDirection;
        public float GroundDistance => _groundDistance;
        public float DesiredGroundDistance => MaxStepHeight;
        public float MaxStepHeight => capsuleStance.ActorHeight * stepHeightRatio;

        public void FixedTick(
            bool useExtendedProbe,
            Vector3 moveDirection,
            float movementProbeOffsetRatio)
        {
            HasGroundHit = TryFindGround(
                useExtendedProbe,
                moveDirection,
                movementProbeOffsetRatio,
                out RaycastHit hit);
            IsGrounded = false;
            IsTooSteep = false;

            if (!HasGroundHit)
            {
                _groundNormal = Vector3.up;
                _groundPoint = _probeOrigin + _probeDirection * _probeDistance;
                _groundDistance = 0f;
                return;
            }

            _groundNormal = hit.normal;
            _groundPoint = hit.point;

            float slopeAngle = Vector3.Angle(_groundNormal, Vector3.up);
            IsTooSteep = slopeAngle > maxSlopeAngle;
            IsGrounded = !IsTooSteep;
        }

        private bool TryFindGround(
            bool useExtendedProbe,
            Vector3 moveDirection,
            float movementProbeOffsetRatio,
            out RaycastHit hit)
        {
            Vector3 capsuleAxis = GetCapsuleAxis();
            CapsuleCollider capsuleCollider = capsuleStance.CapsuleCollider;
            Vector3 center = capsuleCollider.transform.TransformPoint(capsuleCollider.center);
            Vector3 planarMoveDirection = Vector3.ProjectOnPlane(moveDirection, capsuleAxis);

            _probeDirection = -capsuleAxis;
            _sampleCount = 0;
            _selectedSampleIndex = -1;

            float lowerPointDistance = GetWorldHalfHeight();
            float maxStepHeight = MaxStepHeight;
            float groundRange = useExtendedProbe ? maxStepHeight * 2f : maxStepHeight;
            _probeDistance = lowerPointDistance + groundRange + groundProbeOffset;

            if (planarMoveDirection.sqrMagnitude > 0.0001f)
            {
                Vector3 moveNormal = planarMoveDirection.normalized;
                Vector3 forwardOrigin = center + moveNormal * GetMovementProbeOffsetDistance(movementProbeOffsetRatio);

                bool hasForwardHit = TrySampleGround(
                    forwardOrigin,
                    lowerPointDistance,
                    groundRange,
                    out RaycastHit forwardHit,
                    out int forwardSampleIndex);

                if (TrySampleGround(center, lowerPointDistance, groundRange, out RaycastHit centerHit, out int movingCenterSampleIndex))
                {
                    if (hasForwardHit && IsForwardHitHigher(forwardHit, centerHit))
                    {
                        hit = forwardHit;
                        SelectSample(forwardSampleIndex, hit, lowerPointDistance);
                        return true;
                    }

                    hit = centerHit;
                    SelectSample(movingCenterSampleIndex, hit, lowerPointDistance);
                    return true;
                }

                hit = default;
                _probeOrigin = forwardOrigin;
                return false;
            }

            if (TrySampleGround(center, lowerPointDistance, groundRange, out hit, out int centerSampleIndex))
            {
                SelectSample(centerSampleIndex, hit, lowerPointDistance);
                return true;
            }

            hit = default;
            _probeOrigin = center;
            return false;
        }

        private bool IsForwardHitHigher(RaycastHit forwardHit, RaycastHit centerHit)
        {
            float forwardHeight = Vector3.Dot(forwardHit.point, Vector3.up);
            float centerHeight = Vector3.Dot(centerHit.point, Vector3.up);
            return forwardHeight > centerHeight + minForwardStepUpHeight;
        }

        private bool TrySampleGround(
            Vector3 origin,
            float lowerPointDistance,
            float groundRange,
            out RaycastHit hit,
            out int sampleIndex)
        {
            sampleIndex = AddSample(origin);
            bool hasHit = Physics.Raycast(
                origin,
                _probeDirection,
                out hit,
                _probeDistance,
                groundMask,
                QueryTriggerInteraction.Ignore);

            _sampleHits[sampleIndex] = hasHit;

            if (!hasHit)
            {
                hit = default;
                return false;
            }

            float groundDistance = hit.distance - lowerPointDistance;
            bool isValid =
                groundDistance >= -groundProbeOffset &&
                groundDistance <= groundRange + groundProbeOffset &&
                Vector3.Dot(hit.normal, Vector3.up) >= minGroundNormalDot;

            _sampleValid[sampleIndex] = isValid;
            return isValid;
        }

        private int AddSample(Vector3 origin)
        {
            int sampleIndex = _sampleCount;
            _sampleOrigins[sampleIndex] = origin;
            _sampleHits[sampleIndex] = false;
            _sampleValid[sampleIndex] = false;
            _sampleCount++;
            return sampleIndex;
        }

        private void SelectSample(int sampleIndex, RaycastHit hit, float lowerPointDistance)
        {
            _selectedSampleIndex = sampleIndex;
            _probeOrigin = _sampleOrigins[sampleIndex];
            _groundDistance = hit.distance - lowerPointDistance;
        }

        private float GetMovementProbeOffsetDistance(float movementProbeOffsetRatio)
        {
            return GetWorldRadius() * Mathf.Clamp01(movementProbeOffsetRatio);
        }

        private Vector3 GetCapsuleAxis()
        {
            CapsuleCollider capsuleCollider = capsuleStance.CapsuleCollider;
            return capsuleCollider.transform.TransformDirection(GetLocalCapsuleAxis()).normalized;
        }

        private Vector3 GetLocalCapsuleAxis()
        {
            CapsuleCollider capsuleCollider = capsuleStance.CapsuleCollider;
            return capsuleCollider.direction switch
            {
                0 => Vector3.right,
                1 => Vector3.up,
                2 => Vector3.forward,
                _ => Vector3.up
            };
        }

        private float GetWorldRadius()
        {
            CapsuleCollider capsuleCollider = capsuleStance.CapsuleCollider;
            Vector3 scale = capsuleCollider.transform.lossyScale;
            return capsuleCollider.direction switch
            {
                0 => capsuleCollider.radius * Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)),
                1 => capsuleCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)),
                2 => capsuleCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)),
                _ => capsuleCollider.radius
            };
        }

        private float GetWorldHalfHeight()
        {
            CapsuleCollider capsuleCollider = capsuleStance.CapsuleCollider;
            Vector3 scale = capsuleCollider.transform.lossyScale;
            float axisScale = capsuleCollider.direction switch
            {
                0 => Mathf.Abs(scale.x),
                1 => Mathf.Abs(scale.y),
                2 => Mathf.Abs(scale.z),
                _ => 1f
            };

            return capsuleCollider.height * axisScale * 0.5f;
        }

        private void OnDrawGizmosSelected()
        {
            if (capsuleStance == null || capsuleStance.CapsuleCollider == null)
            {
                return;
            }

            CapsuleCollider capsuleCollider = capsuleStance.CapsuleCollider;
            Vector3 capsuleAxis = GetCapsuleAxis();
            Vector3 castOrigin = Application.isPlaying
                ? _probeOrigin
                : capsuleCollider.transform.TransformPoint(capsuleCollider.center);
            float maxStepHeight = MaxStepHeight;
            float castDistance = Application.isPlaying
                ? _probeDistance
                : GetWorldHalfHeight() + maxStepHeight * 2f + groundProbeOffset;

            Gizmos.color = HasGroundHit ? (IsGrounded ? Color.green : Color.yellow) : Color.red;
            
            if (Application.isPlaying)
            {
                for (int i = 0; i < _sampleCount; i++)
                {
                    if (i == _selectedSampleIndex)
                    {
                        Gizmos.color = Color.green;
                    }
                    else if (_sampleValid[i])
                    {
                        Gizmos.color = Color.cyan;
                    }
                    else if (_sampleHits[i])
                    {
                        Gizmos.color = Color.yellow;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }

                    Gizmos.DrawLine(_sampleOrigins[i], _sampleOrigins[i] - capsuleAxis * castDistance);
                }
            }
            else
            {
                Gizmos.DrawLine(castOrigin, castOrigin - capsuleAxis * castDistance);
            }

            if (!HasGroundHit)
            {
                return;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_groundPoint, _groundNormal);
        }
    }
}

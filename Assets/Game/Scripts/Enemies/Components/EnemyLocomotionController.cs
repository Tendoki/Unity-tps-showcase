using Game.Actors.Movement;
using UnityEngine;

namespace Game.Enemies.Components
{
    public class EnemyLocomotionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ActorMotor actorMotor;
        [SerializeField] private ActorGroundDetector groundDetector;

        [Header("Debug")]
        [SerializeField] private bool logRootMotionRotation;

        private Quaternion _navigationRotation;

        private const float NavigationRotationThreshold = 2f;

        public bool IsGrounded => groundDetector.IsGrounded;
        public bool IsSliding => groundDetector.HasGroundHit && groundDetector.IsTooSteep;

        private void Awake()
        {
            if (actorMotor == null)
            {
                actorMotor = GetComponent<ActorMotor>();
            }

            ResetRootMotionRotation(transform.rotation);
        }

        public void ResetRootMotionRotation(Quaternion currentRotation)
        {
            _navigationRotation = currentRotation;
        }

        public void Move(Vector3 targetDirection, float targetSpeed, float movementProbeOffsetRatio, float deltaTime)
        {
            ProbeGround(targetDirection, movementProbeOffsetRatio);
            actorMotor.Move(targetDirection, targetSpeed, deltaTime, groundDetector);
        }

        public void MoveRootMotion(
            Vector3 targetDirection,
            Vector3 rootMotionDeltaPosition,
            float rootMotionDeltaTime,
            float movementProbeOffsetRatio,
            float deltaTime)
        {
            Vector3 planarDelta = Vector3.ProjectOnPlane(
                rootMotionDeltaPosition,
                Vector3.up
            );

            Vector3 rootMotionVelocity = rootMotionDeltaTime > 0f
                ? planarDelta / rootMotionDeltaTime
                : Vector3.zero;

            ProbeGround(rootMotionVelocity.normalized, movementProbeOffsetRatio);

            actorMotor.MoveRootMotion(
                rootMotionVelocity,
                deltaTime,
                groundDetector
            );
        }

        public void Rotate(Quaternion targetRotation, float smoothing, float deltaTime)
        {
            actorMotor.Rotate(targetRotation, smoothing, deltaTime);
        }

        public void RotateRootMotion(
            Vector3 targetDirection,
            Quaternion rootMotionDeltaRotation,
            float smoothing,
            float deltaTime)
        {
            if (targetDirection.sqrMagnitude > 0.0001f)
            {
                Vector3 navigationForward = _navigationRotation * Vector3.forward;
                float angle = Vector3.Angle(navigationForward, targetDirection);

                if (angle > NavigationRotationThreshold)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    float t = 1f - Mathf.Exp(-smoothing * deltaTime);

                    _navigationRotation = Quaternion.Slerp(
                        _navigationRotation,
                        targetRotation,
                        t
                    );

                    _navigationRotation = NormalizeRotation(
                        _navigationRotation,
                        targetRotation
                    );
                }
            }

            Quaternion finalRotation = NormalizeRotation(
                _navigationRotation,
                transform.rotation
            );

            LogRootMotionRotation(finalRotation);

            actorMotor.SetRotation(finalRotation);
        }

        public void Stop()
        {
            Move(Vector3.zero, 0f, movementProbeOffsetRatio: 0f, Time.fixedDeltaTime);
        }

        private void ProbeGround(Vector3 targetDirection, float movementProbeOffsetRatio)
        {
            bool hadGroundContact = groundDetector.HasGroundHit || groundDetector.IsTooSteep;
            groundDetector.FixedTick(
                hadGroundContact,
                targetDirection,
                movementProbeOffsetRatio);
        }

        private static Quaternion NormalizeRotation(Quaternion rotation, Quaternion fallback)
        {
            float magnitude = Mathf.Sqrt(
                rotation.x * rotation.x
                + rotation.y * rotation.y
                + rotation.z * rotation.z
                + rotation.w * rotation.w
            );

            if (magnitude <= 0.0001f)
            {
                return fallback;
            }

            float scale = 1f / magnitude;
            return new Quaternion(
                rotation.x * scale,
                rotation.y * scale,
                rotation.z * scale,
                rotation.w * scale
            );
        }

        private void LogRootMotionRotation(Quaternion finalRotation)
        {
            if (!logRootMotionRotation)
            {
                return;
            }

            Debug.Log(
                $"{name} RootMotionRotation | " +
                $"navigation: {_navigationRotation.eulerAngles} | " +
                $"final: {finalRotation.eulerAngles}",
                this
            );
        }
    }
}

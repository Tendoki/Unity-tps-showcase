using UnityEngine;

namespace Game.Actors.Movement
{
    public class ActorMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private CapsuleCollider capsuleCollider;

        [Header("Movement")]
        [SerializeField] private float accelerationSmoothing = 8f;
        [SerializeField] private float decelerationSmoothing = 4f;
        [SerializeField] private float speedSnapThreshold = 0.01f;

        [Header("Momentum")]
        [SerializeField] private float gravity = 30f;
        [SerializeField] private float airControlRate = 2f;
        [SerializeField] private float airFriction = 0.5f;
        [SerializeField] private float groundFriction = 100f;

        [Header("Ground Adjustment")]
        [SerializeField] private float minGroundStepUpSpeed = 1f;
        [SerializeField] private float maxGroundStepUpSpeed = 8f;
        [SerializeField] private float groundStepUpSpeedMultiplier = 1f;
        [SerializeField] private float minGroundStepDownSpeed = 2f;
        [SerializeField] private float maxGroundStepDownSpeed = 10f;
        [SerializeField] private float groundStepDownSpeedMultiplier = 1.2f;

        [Header("Sliding")]
        [SerializeField] private float slideGravity = 5f;
        [SerializeField] private float slideControlRate = 1f;

        [Header("Rotation")]
        [SerializeField] private float rotationSnapAngle = 0.1f;

        [Header("Physics")]
        [SerializeField] private bool useFrictionlessColliderMaterial = true;
        [SerializeField] private string frictionlessMaterialName = "Character Frictionless";

        private Vector3 _momentum;
        private Vector3 _movementVelocity;
        private Vector3 _groundAdjustmentVelocity;
        private Vector3 _horizontalVelocity;
        private float _speed;
        private float _groundAdjustmentDisableCounter;
        private bool _wasStableGrounded;
        private PhysicsMaterial _frictionlessMaterial;

        public Vector3 Momentum => _momentum;
        public Vector3 MovementVelocity => _movementVelocity;
        public Vector3 GroundAdjustmentVelocity => _groundAdjustmentVelocity;
        public Vector3 HorizontalVelocity => _horizontalVelocity;
        public float Speed => _speed;
        public float Yaw => rb.rotation.eulerAngles.y;

        protected Rigidbody Rigidbody => rb;

        protected virtual void Awake()
        {
            rb.useGravity = false;
            rb.linearDamping = 0f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            if (useFrictionlessColliderMaterial)
            {
                _frictionlessMaterial = new PhysicsMaterial(frictionlessMaterialName)
                {
                    dynamicFriction = 0f,
                    staticFriction = 0f,
                    bounciness = 0f,
                    frictionCombine = PhysicsMaterialCombine.Minimum,
                    bounceCombine = PhysicsMaterialCombine.Minimum
                };

                capsuleCollider.material = _frictionlessMaterial;
            }
        }

        public void Rotate(Quaternion targetRotation, float smoothing, float deltaTime)
        {
            float angle = Quaternion.Angle(rb.rotation, targetRotation);

            if (angle <= rotationSnapAngle)
            {
                rb.MoveRotation(targetRotation);
                return;
            }

            float t = 1f - Mathf.Exp(-smoothing * deltaTime);
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, t);
            rb.MoveRotation(smoothedRotation);
        }

        public void SetRotation(Quaternion rotation)
        {
            rb.MoveRotation(rotation);
        }

        public void Move(
            Vector3 targetDirection,
            float targetSpeed,
            float deltaTime,
            ActorGroundDetector groundDetector,
            IActorMotorModifier modifier = null)
        {
            UpdateGroundAdjustmentTimer(deltaTime);
            OnBeforeMove(deltaTime);
            modifier?.BeforeMove(deltaTime);
            UpdateSpeed(targetSpeed, deltaTime);

            Vector3 requestedMovementVelocity = CalculateRequestedMovementVelocity(targetDirection, _speed);
            MoveWithRequestedVelocity(requestedMovementVelocity, deltaTime, groundDetector, modifier);
        }

        public void MoveRootMotion(
            Vector3 requestedMovementVelocity,
            float deltaTime,
            ActorGroundDetector groundDetector,
            IActorMotorModifier modifier = null)
        {
            UpdateGroundAdjustmentTimer(deltaTime);
            OnBeforeMove(deltaTime);
            modifier?.BeforeMove(deltaTime);
            _speed = Vector3.ProjectOnPlane(requestedMovementVelocity, Vector3.up).magnitude;
            MoveWithRequestedVelocity(requestedMovementVelocity, deltaTime, groundDetector, modifier);
        }

        public void Stop(ActorGroundDetector groundDetector)
        {
            Move(Vector3.zero, 0f, Time.fixedDeltaTime, groundDetector);
        }

        private void MoveWithRequestedVelocity(
            Vector3 requestedMovementVelocity,
            float deltaTime,
            ActorGroundDetector groundDetector,
            IActorMotorModifier modifier)
        {
            bool isStableGrounded = groundDetector.IsGrounded;
            bool isSliding = groundDetector.HasGroundHit && groundDetector.IsTooSteep;

            if (_wasStableGrounded && !isStableGrounded)
            {
                OnGroundContactLost();
            }

            HandleMomentum(
                requestedMovementVelocity,
                deltaTime,
                isStableGrounded,
                isSliding,
                groundDetector.GroundNormal,
                modifier);

            _movementVelocity = isStableGrounded
                ? requestedMovementVelocity
                : Vector3.zero;

            _groundAdjustmentVelocity = CalculateGroundAdjustmentVelocity(groundDetector, deltaTime);

            Vector3 finalVelocity = _movementVelocity + _momentum + _groundAdjustmentVelocity;
            rb.linearVelocity = finalVelocity;

            _horizontalVelocity = Vector3.ProjectOnPlane(_movementVelocity + RemoveDotVector(_momentum, Vector3.up), Vector3.up);
            _wasStableGrounded = isStableGrounded;
            OnAfterMove();
            modifier?.AfterMove();
        }

        public void DisableGroundAdjustment(float duration)
        {
            _groundAdjustmentDisableCounter = Mathf.Max(_groundAdjustmentDisableCounter, duration);
        }

        protected virtual void OnBeforeMove(float deltaTime)
        {
        }

        protected virtual void OnAfterMove()
        {
        }

        protected virtual void OnMomentumUpdated(ref Vector3 momentum, float deltaTime)
        {
        }

        public static Vector3 RemoveDotVector(Vector3 vector, Vector3 direction)
        {
            direction.Normalize();
            return vector - direction * Vector3.Dot(vector, direction);
        }

        private void UpdateSpeed(float targetSpeed, float deltaTime)
        {
            float smoothing = targetSpeed > _speed
                ? accelerationSmoothing
                : decelerationSmoothing;

            float t = Mathf.Clamp01(smoothing * deltaTime);
            _speed = Mathf.Lerp(_speed, targetSpeed, t);

            if (Mathf.Abs(targetSpeed - _speed) <= speedSnapThreshold)
            {
                _speed = targetSpeed;
            }
        }

        private static Vector3 CalculateRequestedMovementVelocity(Vector3 targetDirection, float speed)
        {
            if (targetDirection.sqrMagnitude <= 0.0001f || speed <= 0f)
            {
                return Vector3.zero;
            }

            return targetDirection.normalized * speed;
        }

        private void HandleMomentum(
            Vector3 requestedMovementVelocity,
            float deltaTime,
            bool isStableGrounded,
            bool isSliding,
            Vector3 groundNormal,
            IActorMotorModifier modifier)
        {
            Vector3 verticalMomentum = ExtractDotVector(_momentum, Vector3.up);
            Vector3 horizontalMomentum = _momentum - verticalMomentum;

            verticalMomentum -= Vector3.up * (gravity * deltaTime);

            if (isStableGrounded && Vector3.Dot(verticalMomentum, Vector3.up) < 0f)
            {
                verticalMomentum = Vector3.zero;
            }

            if (!isStableGrounded && !isSliding)
            {
                AdjustAirMomentum(ref horizontalMomentum, requestedMovementVelocity, deltaTime);
            }

            if (isSliding)
            {
                AdjustSlidingMomentum(ref horizontalMomentum, requestedMovementVelocity, groundNormal, deltaTime);
            }

            float friction = isStableGrounded ? groundFriction : airFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * deltaTime);

            _momentum = horizontalMomentum + verticalMomentum;
            OnMomentumUpdated(ref _momentum, deltaTime);
            modifier?.ModifyMomentum(ref _momentum, this, deltaTime);

            if (isSliding)
            {
                ProjectMomentumOntoSlope(groundNormal, deltaTime);
            }
        }

        private void AdjustAirMomentum(
            ref Vector3 horizontalMomentum,
            Vector3 requestedMovementVelocity,
            float deltaTime)
        {
            if (requestedMovementVelocity.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float currentSpeed = horizontalMomentum.magnitude;

            if (currentSpeed > 0.0001f)
            {
                Vector3 currentDirection = horizontalMomentum / currentSpeed;

                if (Vector3.Dot(requestedMovementVelocity, currentDirection) > 0f)
                {
                    requestedMovementVelocity = RemoveDotVector(requestedMovementVelocity, currentDirection);
                }
            }

            horizontalMomentum += requestedMovementVelocity * (deltaTime * airControlRate);
            horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, Mathf.Max(currentSpeed, _speed));
        }

        private void AdjustSlidingMomentum(
            ref Vector3 horizontalMomentum,
            Vector3 requestedMovementVelocity,
            Vector3 groundNormal,
            float deltaTime)
        {
            if (requestedMovementVelocity.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 uphillDirection = Vector3.ProjectOnPlane(groundNormal, Vector3.up).normalized;
            Vector3 slideControlVelocity = RemoveDotVector(requestedMovementVelocity, uphillDirection);
            horizontalMomentum += slideControlVelocity * (deltaTime * slideControlRate);
        }

        private void ProjectMomentumOntoSlope(Vector3 groundNormal, float deltaTime)
        {
            _momentum = Vector3.ProjectOnPlane(_momentum, groundNormal);

            if (Vector3.Dot(_momentum, Vector3.up) > 0f)
            {
                _momentum = RemoveDotVector(_momentum, Vector3.up);
            }

            Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            _momentum += slideDirection * (slideGravity * deltaTime);
        }

        private Vector3 CalculateGroundAdjustmentVelocity(
            ActorGroundDetector groundDetector,
            float deltaTime)
        {
            if (!groundDetector.HasGroundHit || _groundAdjustmentDisableCounter > 0f || deltaTime <= 0f)
            {
                return Vector3.zero;
            }

            float groundDistance = groundDetector.GroundDistance;
            Vector3 adjustmentDirection = -groundDetector.ProbeDirection;
            float distanceToGo = groundDetector.DesiredGroundDistance - groundDistance;
            float targetSpeed = distanceToGo / deltaTime;
            float horizontalSpeed = Vector3.ProjectOnPlane(_movementVelocity + _momentum, Vector3.up).magnitude;
            float maxSpeed = distanceToGo > 0f
                ? CalculateGroundStepSpeed(horizontalSpeed, minGroundStepUpSpeed, maxGroundStepUpSpeed, groundStepUpSpeedMultiplier)
                : CalculateGroundStepSpeed(horizontalSpeed, minGroundStepDownSpeed, maxGroundStepDownSpeed, groundStepDownSpeedMultiplier);
            float clampedSpeed = Mathf.Clamp(targetSpeed, -maxSpeed, maxSpeed);

            return adjustmentDirection * clampedSpeed;
        }

        private static float CalculateGroundStepSpeed(
            float horizontalSpeed,
            float minSpeed,
            float maxSpeed,
            float speedMultiplier)
        {
            float speed = horizontalSpeed * speedMultiplier;
            return Mathf.Clamp(speed, minSpeed, maxSpeed);
        }

        private void OnGroundContactLost()
        {
            Vector3 verticalMomentum = ExtractDotVector(_momentum, Vector3.up);
            Vector3 horizontalMomentum = _momentum - verticalMomentum;
            Vector3 groundVelocity = _movementVelocity;

            if (groundVelocity.sqrMagnitude > horizontalMomentum.sqrMagnitude)
            {
                horizontalMomentum = groundVelocity;
            }

            _momentum = horizontalMomentum + verticalMomentum;
        }

        private void UpdateGroundAdjustmentTimer(float deltaTime)
        {
            if (_groundAdjustmentDisableCounter <= 0f)
            {
                return;
            }

            _groundAdjustmentDisableCounter -= deltaTime;
        }

        private static Vector3 ExtractDotVector(Vector3 vector, Vector3 direction)
        {
            direction.Normalize();
            return direction * Vector3.Dot(vector, direction);
        }
        
        public void RotateRootMotion(
            Quaternion targetRotation,
            Quaternion rootMotionRotation,
            float smoothing,
            float deltaTime)
        {
            float t = 1f - Mathf.Exp(-smoothing * deltaTime);

            Quaternion navRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                t
            );

            Quaternion finalRotation = navRotation * rootMotionRotation;

            rb.MoveRotation(finalRotation);
        }
    }
}

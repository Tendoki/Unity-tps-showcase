using Game.Player.ThirdPerson.Camera;
using Game.Actors.Movement;
using System;
using Game.FSM;
using Game.Scripts.Services.Input;
using Game.Services;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Player.ThirdPerson.Movement
{
    public class TPLocomotionController : MonoBehaviour, ITPLocomotionStateActions
    {
        [Header("References")]
        [SerializeField] private TPPlayerController playerController;
        [SerializeField] private TPCameraController cameraController;
        [SerializeField] private ActorGroundDetector groundDetector;
        [SerializeField] private PlayerLocomotionMotor mover;
        [SerializeField] private PlayerCrouchController crouchController;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float crouchSpeed = 2.5f;

        [Header("Air Control")]
        [SerializeField] private float airControlSpeed = 5f;

        [Header("Ground Probe")]
        [Range(0f, 1f)]
        [SerializeField] private float walkGroundProbeOffsetRatio = 0.45f;
        [Range(0f, 1f)]
        [SerializeField] private float sprintGroundProbeOffsetRatio = 0.65f;
        [Range(0f, 1f)]
        [SerializeField] private float crouchGroundProbeOffsetRatio = 0.35f;
        [Range(0f, 1f)]
        [SerializeField] private float airGroundProbeOffsetRatio = 0.45f;

        [Header("Rotation")]
        [SerializeField] private float movementRotationSmoothing = 14f;
        [SerializeField] private float aimingRotationSmoothing = 40f;

        [Header("Jump")]
        [SerializeField] private float coyoteTime = 0.15f;

        private IInputService _inputService;
        private StateMachine _physicalStateMachine;
        private Vector3 _moveDirection;
        private Quaternion _targetRotation;
        private float _targetRotationSmoothing;
        private float _coyoteCounter;
        private bool _hasTargetRotation;

        public event Action Jumped;

        public float Speed => mover.Speed;
        public Vector3 HorizontalVelocity => mover.HorizontalVelocity;
        public IState CurrentPhysicalState => _physicalStateMachine.CurrentState;
        public bool IsGrounded => _physicalStateMachine.CurrentState is GroundedPhysicalState;
        public bool UsesGroundedAnimation => _physicalStateMachine.CurrentState is GroundedPhysicalState or CoyotePhysicalState;
        public bool IsFalling => !IsGrounded;
        public bool IsCrouching => crouchController != null && crouchController.IsCrouching;
        private bool IsAiming => playerController != null && playerController.IsAiming;
        private float AimYaw => cameraController != null ? cameraController.CameraYaw : transform.eulerAngles.y;

        private void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
            SetupPhysicalStateMachine();
        }

        public void Tick()
        {
            _physicalStateMachine.Update();
        }

        public void FixedTick()
        {
            _physicalStateMachine.FixedUpdate();
        }

        public void UpdateGroundedControls()
        {
            UpdateMoveDirection();
            UpdateRotationTarget();
            UpdateCrouch(canEnterCrouch: true);
        }

        public void UpdateAirborneControls(bool canEnterCrouch)
        {
            UpdateMoveDirection();
            UpdateRotationTarget();
            UpdateCrouch(canEnterCrouch);
        }

        public void SimulateGroundedMovement()
        {
            bool hadGroundContact = groundDetector.IsGrounded || groundDetector.IsTooSteep;
            bool useExtendedProbe = hadGroundContact && !mover.HasPendingJump;
            float maxSpeed = ResolveMaxMovementSpeed();
            float targetSpeed = ResolveSpeed(maxSpeed);

            Rotate();
            ProbeGround(useExtendedProbe, ResolveGroundProbeOffsetRatio());
            Move(targetSpeed);
            ResetCoyoteTime();
        }

        public void SimulateAirborneMovement()
        {
            SimulateAirborne(useExtendedProbe: false);
        }

        public void SimulateFallingMovement()
        {
            bool hadGroundHit = groundDetector.HasGroundHit;
            SimulateAirborne(useExtendedProbe: hadGroundHit);
        }

        public bool TryStartJump()
        {
            if (!HasJumpRequest())
            {
                return false;
            }
            
            Jumped?.Invoke();
            mover.RequestJump();
            _coyoteCounter = 0f;
            return true;
        }

        private void SetupPhysicalStateMachine()
        {
            _physicalStateMachine = new StateMachine();

            var grounded = new GroundedPhysicalState(this);
            var coyote = new CoyotePhysicalState(this);
            var jumping = new JumpingPhysicalState(this);
            var rising = new RisingPhysicalState(this);
            var falling = new FallingPhysicalState(this);

            At(grounded, jumping, HasJumpRequest);
            At(grounded, coyote, () => !groundDetector.IsGrounded);

            At(coyote, grounded, () => groundDetector.IsGrounded);
            At(coyote, jumping, HasJumpRequest);
            At(coyote, falling, ShouldFall);

            At(jumping, grounded, () => !IsJumpInProgress() && groundDetector.IsGrounded);
            At(jumping, rising, () => !IsJumpInProgress() && IsRising());
            At(jumping, falling, () => !IsJumpInProgress() && !IsRising());

            At(rising, grounded, () => groundDetector.IsGrounded);
            At(rising, falling, IsMovingDown);

            At(falling, grounded, () => groundDetector.IsGrounded);

            _physicalStateMachine.SetState(falling);
        }

        private void At(IState from, IState to, Func<bool> condition)
        {
            _physicalStateMachine.AddTransition(from, to, new FuncPredicate(condition));
        }

        private bool HasJumpRequest()
        {
            return _inputService.JumpPressedThisFrame && CanJump();
        }

        private bool CanJump()
        {
            return !IsCrouching
                   && !IsAiming
                   && HasGroundedJumpWindow();
        }

        private bool ShouldFall()
        {
            return !groundDetector.IsGrounded && _coyoteCounter <= 0f;
        }

        private bool IsJumpInProgress()
        {
            return mover.IsJumpRequested || mover.IsJumping;
        }

        private bool IsRising()
        {
            return Vector3.Dot(mover.Momentum, Vector3.up) > 0.01f;
        }

        private bool IsMovingDown()
        {
            return Vector3.Dot(mover.Momentum, Vector3.up) <= 0.01f;
        }

        private void SimulateAirborne(bool useExtendedProbe)
        {
            float targetSpeed = ResolveSpeed(airControlSpeed);

            Rotate();
            ProbeGround(useExtendedProbe, airGroundProbeOffsetRatio);
            Move(targetSpeed);
            ReduceCoyoteTime();
        }

        private void UpdateMoveDirection()
        {
            Vector3 forward = Vector3.ProjectOnPlane(cameraController.CameraPivot.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraController.CameraPivot.right, Vector3.up).normalized;

            _moveDirection = forward * _inputService.MoveInput.y + right * _inputService.MoveInput.x;
            _moveDirection = Vector3.ClampMagnitude(_moveDirection, 1f);

            if (_moveDirection.sqrMagnitude < 0.001f)
            {
                _moveDirection = Vector3.zero;
            }
        }

        private void UpdateRotationTarget()
        {
            if (ShouldRotateByAim())
            {
                _targetRotation = Quaternion.Euler(0f, AimYaw, 0f);
                _targetRotationSmoothing = aimingRotationSmoothing;
                _hasTargetRotation = true;
                return;
            }

            if (_moveDirection.sqrMagnitude <= 0.0001f)
            {
                _hasTargetRotation = false;
                return;
            }

            _targetRotation = Quaternion.LookRotation(_moveDirection);
            _targetRotationSmoothing = movementRotationSmoothing;
            _hasTargetRotation = true;
        }

        private void Rotate()
        {
            if (!_hasTargetRotation)
            {
                return;
            }

            mover.Rotate(_targetRotation, _targetRotationSmoothing, Time.fixedDeltaTime);
        }

        private void ProbeGround(bool useExtendedProbe, float movementProbeOffsetRatio)
        {
            groundDetector.FixedTick(
                useExtendedProbe,
                _moveDirection,
                movementProbeOffsetRatio);
        }

        private void Move(float targetSpeed)
        {
            mover.Move(
                _moveDirection,
                targetSpeed,
                Time.fixedDeltaTime,
                groundDetector,
                _inputService.JumpHeld);
        }

        private void UpdateCrouch(bool canEnterCrouch)
        {
            crouchController.Tick(
                _inputService.CrouchHeld,
                CanEnterCrouch(canEnterCrouch),
                IsAiming,
                Time.deltaTime);
        }

        private float ResolveMaxMovementSpeed()
        {
            if (IsCrouching)
            {
                return crouchSpeed;
            }

            if (ShouldUseAimingMovement())
            {
                return walkSpeed;
            }

            if (CanSprint())
            {
                return sprintSpeed;
            }

            return walkSpeed;
        }

        private float ResolveGroundProbeOffsetRatio()
        {
            if (IsCrouching)
            {
                return crouchGroundProbeOffsetRatio;
            }

            if (ShouldUseAimingMovement())
            {
                return walkGroundProbeOffsetRatio;
            }

            if (CanSprint())
            {
                return sprintGroundProbeOffsetRatio;
            }

            return walkGroundProbeOffsetRatio;
        }

        private float ResolveSpeed(float maxSpeed)
        {
            return _moveDirection.sqrMagnitude < 0.001f ? 0f : maxSpeed;
        }

        private void ResetCoyoteTime()
        {
            _coyoteCounter = coyoteTime;
        }

        private void ReduceCoyoteTime()
        {
            _coyoteCounter -= Time.fixedDeltaTime;
        }

        private bool HasGroundedJumpWindow()
        {
            return groundDetector.IsGrounded || _coyoteCounter > 0f;
        }

        private bool CanEnterCrouch(bool stateAllowsCrouch)
        {
            return stateAllowsCrouch && !IsAiming;
        }

        private bool CanSprint()
        {
            return _inputService.SprintHeld && !IsCrouching && !IsAiming;
        }

        private bool ShouldUseAimingMovement()
        {
            return IsAiming;
        }

        private bool ShouldRotateByAim()
        {
            return IsAiming;
        }
    }
}

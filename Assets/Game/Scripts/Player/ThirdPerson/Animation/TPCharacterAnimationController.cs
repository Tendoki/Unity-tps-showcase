using Game.Player;
using Game.Player.ThirdPerson.Movement;
using Game.Player.ThirdPerson.Camera;
using Game.Player.ThirdPerson;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

namespace Game.Player.ThirdPerson.Animation
{
    public class TPCharacterAnimationController : MonoBehaviour, IHeldItemAnimationRig
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform characterTransform;
        [FormerlySerializedAs("characterController")]
        [SerializeField] private TPPlayerController playerController;
        [SerializeField] private TPCameraController cameraController;
        [FormerlySerializedAs("movementController")]
        [SerializeField] private TPLocomotionController locomotionController;

        [Header("Held Item Rig")]
        [SerializeField] private TwoBoneIKConstraint leftHandIk;

        [Header("Weapon Aim Rig")]
        [SerializeField] private Transform aimTarget;
        [SerializeField] private Transform spineAimTarget;
        [SerializeField] private Vector3 spineAimOffsetLocal = new(0f, -0.25f, 0f);
        [SerializeField] private Transform rightForearmAimTarget;
        [SerializeField] private Vector3 rightForearmAimOffsetLocal = new(0f, -0.1f, 0f);
        [SerializeField] private float aimTargetDistance = 20f;
        [SerializeField] private float aimTargetLerpSpeed = 20f;
        [SerializeField] private float aimRigBlendSpeed = 10f;
        [SerializeField] private MultiAimConstraint headAim;
        [SerializeField] private MultiAimConstraint spineAim;
        [SerializeField] private MultiAimConstraint chestAim;
        [SerializeField] private MultiAimConstraint upperChestAim;
        [SerializeField] private MultiAimConstraint rightForearmAim;
        [SerializeField] private MultiAimConstraint rightHandAim;
        [SerializeField, Range(0f, 1f)] private float headAimMaxWeight = 0.7f;
        [SerializeField, Range(0f, 1f)] private float spineAimMaxWeight = 0.2f;
        [SerializeField, Range(0f, 1f)] private float chestAimMaxWeight = 0.35f;
        [SerializeField, Range(0f, 1f)] private float upperChestAimMaxWeight = 0.5f;
        [SerializeField, Range(0f, 1f)] private float rightForearmAimMaxWeight = 0.15f;
        [SerializeField, Range(0f, 1f)] private float rightHandAimMaxWeight = 0.1f;
        [SerializeField] private float fullAimWeightYaw = 30f;
        [SerializeField] private float zeroAimWeightYaw = 90f;

        [Header("Damping")]
        [SerializeField] private float velocityDampTime = 0.1f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
        private static readonly int VelocityZHash = Animator.StringToHash("VelocityZ");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
        private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");
        private static readonly int JumpHash = Animator.StringToHash("Jump");

        private float _aimRigWeight;
        private Transform _leftHandIkSource;
        private float _leftHandIkWeight;

        private void OnEnable()
        {
            locomotionController.Jumped += OnJumped;
        }

        private void OnDisable()
        {
            locomotionController.Jumped -= OnJumped;
        }

        public void Tick()
        {
            UpdateAnimatorParameters();
            float yawAimWeight = CalculateYawAimWeight();
            UpdateHeldItemRig(yawAimWeight);
            UpdateWeaponAimRig(yawAimWeight);
        }

        public void SetLeftHandIkTarget(Transform target, float weight)
        {
            if (target == null)
            {
                ClearLeftHandIk();
                return;
            }

            _leftHandIkSource = target;
            _leftHandIkWeight = Mathf.Clamp01(weight);
        }

        public void ClearLeftHandIk()
        {
            _leftHandIkSource = null;
            _leftHandIkWeight = 0f;
        }

        private void UpdateAnimatorParameters()
        {
            Vector3 velocity = characterTransform.InverseTransformDirection(locomotionController.HorizontalVelocity);
            
            animator.SetFloat(SpeedHash, locomotionController.Speed);
            animator.SetFloat(VelocityXHash, velocity.x, velocityDampTime, Time.deltaTime);
            animator.SetFloat(VelocityZHash, velocity.z, velocityDampTime, Time.deltaTime);
            animator.SetBool(IsGroundedHash, locomotionController.UsesGroundedAnimation);
            animator.SetBool(IsCrouchingHash, locomotionController.IsCrouching);
            animator.SetBool(IsAimingHash, playerController.IsAiming);
        }

        private void UpdateHeldItemRig(float yawAimWeight)
        {
            if (!playerController.IsAiming)
            {
                leftHandIk.weight = 0f;
                return;
            }
            
            leftHandIk.data.target.SetPositionAndRotation(_leftHandIkSource.position, _leftHandIkSource.rotation);
            leftHandIk.weight = _leftHandIkWeight * yawAimWeight;
        }

        private void UpdateWeaponAimRig(float yawAimWeight)
        {
            bool isAiming = playerController.IsAiming;
            float targetWeight = isAiming ? 1f : 0f;
            _aimRigWeight = Mathf.MoveTowards(_aimRigWeight, targetWeight, aimRigBlendSpeed * Time.deltaTime);

            UpdateAimTarget(isAiming);

            float finalAimWeight = _aimRigWeight * yawAimWeight;
            SetAimWeight(headAim, finalAimWeight * headAimMaxWeight);
            SetAimWeight(spineAim, finalAimWeight * spineAimMaxWeight);
            SetAimWeight(chestAim, finalAimWeight * chestAimMaxWeight);
            SetAimWeight(upperChestAim, finalAimWeight * upperChestAimMaxWeight);
            SetAimWeight(rightForearmAim, finalAimWeight * rightForearmAimMaxWeight);
            SetAimWeight(rightHandAim, finalAimWeight * rightHandAimMaxWeight);
        }

        private void UpdateAimTarget(bool isAiming)
        {
            if (!isAiming)
                return;

            Transform aimCamera = cameraController.CameraTransform;
            Vector3 targetPosition = aimCamera.position + aimCamera.forward * aimTargetDistance;
            Vector3 spineTargetPosition = targetPosition + aimCamera.rotation * spineAimOffsetLocal;
            Vector3 rightForearmTargetPosition = targetPosition + aimCamera.rotation * rightForearmAimOffsetLocal;
            float t = 1f - Mathf.Exp(-aimTargetLerpSpeed * Time.deltaTime);

            aimTarget.position = Vector3.Lerp(aimTarget.position, targetPosition, t);
            spineAimTarget.position = Vector3.Lerp(spineAimTarget.position, spineTargetPosition, t);
            rightForearmAimTarget.position = Vector3.Lerp(rightForearmAimTarget.position, rightForearmTargetPosition, t);
        }

        private void SetAimWeight(MultiAimConstraint constraint, float weight)
        {
            if (constraint == null)
                return;

            constraint.weight = weight;
        }

        private float CalculateYawAimWeight()
        {
            if (!playerController.IsAiming || cameraController == null || characterTransform == null)
                return 1f;

            float deltaYaw = Mathf.Abs(Mathf.DeltaAngle(characterTransform.eulerAngles.y, cameraController.CameraYaw));
            return Mathf.InverseLerp(zeroAimWeightYaw, fullAimWeightYaw, deltaYaw);
        }

        private void OnJumped()
        {
            animator.SetTrigger(JumpHash);
        }
    }
}

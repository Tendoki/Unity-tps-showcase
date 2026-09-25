using Game.Scripts.Services.Input;
using Game.Services;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Player.ThirdPerson.Camera
{
    public class TPCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private CinemachineCamera followCinemachineCam;
        [SerializeField] private CinemachineCamera aimCinemachineCam;

        [Header("Look")]
        [FormerlySerializedAs("sensitivity")]
        [SerializeField] private float standaloneSensitivity = 0.1f;
        [SerializeField] private float mobileSensitivity = 3f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 70f;

        private IInputService _inputService;
        
        private float _cameraYaw;
        private float _cameraPitch;

        public Transform CameraPivot => cameraPivot;
        public Transform CameraTransform => cameraTransform;
        public float CameraYaw => _cameraYaw;

        private void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
            
            _cameraYaw = cameraPivot.rotation.eulerAngles.y;
            SetAimCameraActive(false);
        }

        public void SetAimCameraActive(bool active)
        {
            followCinemachineCam.Priority = active ? 50 : 100;
            aimCinemachineCam.Priority = active ? 100 : 50;
        }

        public void ReadLookInput()
        {
            Vector2 lookDelta = _inputService.LookInput * CurrentSensitivity;

            _cameraYaw += lookDelta.x;
            _cameraPitch -= lookDelta.y;
            _cameraPitch = Mathf.Clamp(_cameraPitch, minPitch, maxPitch);
        }

        public void ApplyLookTransform()
        {
            Quaternion targetWorldRotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
            cameraPivot.localRotation = Quaternion.Inverse(cameraPivot.parent.rotation) * targetWorldRotation;
        }

        private float CurrentSensitivity =>
            _inputService is MobileInputService
                ? mobileSensitivity
                : standaloneSensitivity;
    }
}

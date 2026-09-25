using Game.Actors.Movement;
using UnityEngine;

namespace Game.Player.ThirdPerson.Movement
{
    public class PlayerLocomotionMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ActorMotor actorMotor;
        [SerializeField] private PlayerJumpController jumpController;

        public Vector3 Momentum => actorMotor.Momentum;
        public Vector3 HorizontalVelocity => actorMotor.HorizontalVelocity;
        public float Speed => actorMotor.Speed;
        public bool IsJumpRequested => jumpController.IsJumpRequested;
        public bool IsJumping => jumpController.IsJumping;
        public bool HasPendingJump => jumpController.HasPendingJump;

        private void Awake()
        {
            if (actorMotor == null)
            {
                actorMotor = GetComponent<ActorMotor>();
            }

            if (jumpController == null)
            {
                jumpController = GetComponent<PlayerJumpController>();
            }
        }

        public void Rotate(Quaternion targetRotation, float smoothing, float deltaTime)
        {
            actorMotor.Rotate(targetRotation, smoothing, deltaTime);
        }

        public void Move(
            Vector3 targetDirection,
            float targetSpeed,
            float deltaTime,
            ActorGroundDetector groundDetector,
            bool jumpHeld)
        {
            jumpController.SetInput(jumpHeld);
            actorMotor.Move(targetDirection, targetSpeed, deltaTime, groundDetector, jumpController);
        }

        public void RequestJump()
        {
            jumpController.RequestJump();
        }
    }
}

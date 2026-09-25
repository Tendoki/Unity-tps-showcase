using Game.Actors.Movement;
using UnityEngine;

namespace Game.Player.ThirdPerson.Movement
{
    public class PlayerJumpController : MonoBehaviour, IActorMotorModifier
    {
        [SerializeField] private float jumpSpeed = 5f;
        [SerializeField] private float jumpDuration = 0.2f;
        [SerializeField] private float groundAdjustmentDisableAfterJumpTime = 0.12f;

        private float _jumpTimer;
        private bool _jumpRequested;
        private bool _isJumping;
        private bool _jumpHeld;

        public bool IsJumpRequested => _jumpRequested;
        public bool IsJumping => _isJumping;
        public bool HasPendingJump => _jumpRequested || _isJumping;

        public void SetInput(bool jumpHeld)
        {
            _jumpHeld = jumpHeld;
        }

        public void RequestJump()
        {
            _jumpRequested = true;
        }

        public void BeforeMove(float deltaTime)
        {
            UpdateJumpState(deltaTime);
        }

        public void ModifyMomentum(ref Vector3 momentum, ActorMotor motor, float deltaTime)
        {
            if (_jumpRequested)
            {
                StartJumping(motor);
            }

            if (!_isJumping)
            {
                return;
            }

            momentum = ActorMotor.RemoveDotVector(momentum, Vector3.up);
            momentum += Vector3.up * jumpSpeed;
            motor.DisableGroundAdjustment(groundAdjustmentDisableAfterJumpTime);
        }

        public void AfterMove()
        {
            _jumpRequested = false;
        }

        private void StartJumping(ActorMotor motor)
        {
            _isJumping = true;
            _jumpTimer = jumpDuration;
            motor.DisableGroundAdjustment(jumpDuration);
        }

        private void UpdateJumpState(float deltaTime)
        {
            if (!_isJumping)
            {
                return;
            }

            _jumpTimer -= deltaTime;

            if (_jumpTimer <= 0f || !_jumpHeld)
            {
                _isJumping = false;
                _jumpTimer = 0f;
            }
        }
    }
}

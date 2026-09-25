using UnityEngine;

namespace Game.Scripts.Services.Input
{
    public abstract class InputService : IInputService
    {
        private readonly GameInputActions _inputActions;

        public Vector2 LookInput { get; protected set; }
        public Vector2 MoveInput { get; protected set; }

        public bool SprintHeld { get; protected set; }
        public bool CrouchHeld { get; protected set; }

        public bool JumpHeld { get; protected set; }
        public bool JumpPressedThisFrame { get; protected set; }

        public bool InteractPressedThisFrame { get; protected set; }
        
        public bool DropPressedThisFrame { get; protected set; }

        public bool AimPressedThisFrame { get; protected set; }

        public InputService()
        {
            _inputActions = new GameInputActions();
            _inputActions.Player.Enable();
        }

        public virtual void Tick()
        {
            MoveInput = _inputActions.Player.Move.ReadValue<Vector2>();
            LookInput = _inputActions.Player.Look.ReadValue<Vector2>();

            SprintHeld = _inputActions.Player.Sprint.IsPressed();
            CrouchHeld = _inputActions.Player.Crouch.IsPressed();

            JumpPressedThisFrame = _inputActions.Player.Jump.WasPressedThisFrame();
            JumpHeld = _inputActions.Player.Jump.IsPressed();

            InteractPressedThisFrame = _inputActions.Player.Interact.WasPressedThisFrame();
            
            DropPressedThisFrame = _inputActions.Player.Drop.WasPressedThisFrame();

            AimPressedThisFrame = _inputActions.Player.Aim.WasPressedThisFrame();
        }

        public void ResetTransientState()
        {
            JumpPressedThisFrame = false;
            InteractPressedThisFrame = false;
            DropPressedThisFrame = false;
            AimPressedThisFrame = false;
        }
    }
}

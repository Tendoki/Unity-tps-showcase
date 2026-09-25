using Game.Services;
using UnityEngine;

namespace Game.Scripts.Services.Input
{
    public interface IInputService : IService
    {
        Vector2 LookInput { get; }
        Vector2 MoveInput { get; }

        bool SprintHeld { get; }
        bool CrouchHeld { get; }

        bool JumpHeld { get; }
        bool JumpPressedThisFrame { get; }

        bool InteractPressedThisFrame { get; }

        bool DropPressedThisFrame { get; }

        bool AimPressedThisFrame { get; }

        void Tick();
        void ResetTransientState();
    }
}

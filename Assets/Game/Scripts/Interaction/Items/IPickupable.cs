using Game.Player;
using UnityEngine;

namespace Game.Interaction.Items
{
    public interface IPickupable
    {
        public Rigidbody Rigidbody { get; }
        public Collider ItemCollider { get; }
        public Vector3 HoldPositionOffset { get; }
        public Vector3 HoldRotationOffset { get; }
        
        public bool CanBePickedUp();
        public void OnPickedUp(ICharacterController character);
        public void OnDropped(ICharacterController character);
        public void ApplyHeldPose(IHeldItemAnimationRig rig);
        public void ClearHeldPose(IHeldItemAnimationRig rig);
    }
}

using Game.Player;
using UnityEngine;

namespace Game.Interaction.Items
{
    public abstract class PickupableItem : Item, IPickupable
    {
        [SerializeField] protected Rigidbody rb;
        [SerializeField] protected Vector3 holdPositionOffset;
        [SerializeField] protected Vector3 holdRotationOffset;
        
        protected bool _pickedUp;

        public Rigidbody Rigidbody => rb;
        public Collider ItemCollider => itemCollider;
        public Vector3 HoldPositionOffset => holdPositionOffset;
        public Vector3 HoldRotationOffset => holdRotationOffset;
        
        public override bool IsInteractible()
        {
            return !_blockInteraction && !_pickedUp;
        }

        public abstract override void Interact();
        
        public abstract bool CanBePickedUp();

        public void OnPickedUp(ICharacterController character)
        {
            _pickedUp = true;
        }

        public void OnDropped(ICharacterController character)
        {
            _pickedUp = false;
        }

        public abstract void ApplyHeldPose(IHeldItemAnimationRig rig);

        public abstract void ClearHeldPose(IHeldItemAnimationRig rig);
    }
}

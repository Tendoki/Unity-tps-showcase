using Game.Player;
using UnityEngine;

namespace Game.Interaction.Items
{
    public class Rifle : PickupableItem, IWeapon
    {
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private Transform leftHandIkTarget;
        [SerializeField, Range(0f, 1f)] private float leftHandIkWeight = 1f;

        public Transform MuzzlePoint => muzzlePoint;
        
        public override void Interact()
        {
            Debug.Log("Interact");
        }

        public override bool CanBePickedUp()
        {
            Debug.Log("CanBePickedUp");
            return true;
        }

        public override void ApplyHeldPose(IHeldItemAnimationRig rig)
        {
            rig.SetLeftHandIkTarget(leftHandIkTarget, leftHandIkWeight);
        }

        public override void ClearHeldPose(IHeldItemAnimationRig rig)
        {
            rig.ClearLeftHandIk();
        }

        public override void HoverEnter()
        {
            Debug.Log("HoverEnter");
        }

        public override void HoverExit()
        {
            Debug.Log("HoverExit");
        }

        
        public void Fire()
        {
            
        }
    }
}

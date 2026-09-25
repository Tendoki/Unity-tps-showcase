using Game.Player;
using Game.Player.ThirdPerson.Animation;
using Game.Player.ThirdPerson;
using Game.Interaction.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Player.ThirdPerson.Interaction
{
    public class TPHeldItemController: MonoBehaviour
    {
        [FormerlySerializedAs("playerController")]
        [FormerlySerializedAs("characterController")]
        [Header("References")]
        [SerializeField] private TPPlayerController playerController;
        [SerializeField] private TPCharacterAnimationController animationController;
        [SerializeField] private Transform rightHandTarget;

        [Header("Pickup")]
        [SerializeField] private float pickupCooldown = 0.2f;
        [SerializeField] private float dropForwardForce = 1.5f;
        
        private float lastPickupTime = float.NegativeInfinity;
        private IPickupable currentPickup;

        public IPickupable CurrentPickup => currentPickup;
        public bool HasCurrentPickup => currentPickup != null;

        public void FixedTick()
        {
        }
        
        private bool CanPickup(IPickupable pickupable)
        {
            if (pickupable == null)
                return false;

            if (!pickupable.CanBePickedUp())
                return false;

            if (Time.time < lastPickupTime + pickupCooldown)
                return false;

            if (currentPickup != null)
                return false;

            return true;
        }

        public bool DropCurrent()
        {
            if (currentPickup == null)
                return false;

            IPickupable droppedPickup = currentPickup;
            currentPickup = null;

            Rigidbody droppedBody = droppedPickup.Rigidbody;
            Transform droppedTransform = droppedBody.transform;
            
            droppedPickup.ClearHeldPose(animationController);
            droppedTransform.SetParent(null, true);
            RestoreDropped(droppedPickup);
            droppedPickup.OnDropped(playerController);
            droppedBody.AddForce(rightHandTarget.forward * dropForwardForce, ForceMode.VelocityChange);

            lastPickupTime = Time.time;
            return true;
        }
        
        public bool TryPickup(IPickupable pickupable)
        {
            if (!CanPickup(pickupable))
                return false;

            lastPickupTime = Time.time;
            currentPickup = pickupable;
            pickupable.OnPickedUp(playerController);
            ConfigurePickup(pickupable);
            AttachToRightHand(pickupable);
            pickupable.ApplyHeldPose(animationController);

            if (pickupable is IEquippable equippable)
            {
                // Позже здесь будет:
                // inventory.Add(equippable);
                // equippedItems.Equip(equippable);
            }

            return true;
        }

        private void AttachToRightHand(IPickupable pickupable)
        {
            if (rightHandTarget == null)
                return;

            Transform itemTransform = pickupable.Rigidbody.transform;
            itemTransform.SetParent(rightHandTarget, false);
            itemTransform.localPosition = pickupable.HoldPositionOffset;
            itemTransform.localRotation = Quaternion.Euler(pickupable.HoldRotationOffset);
        }

        private void ConfigurePickup(IPickupable pickupable)
        {
            pickupable.Rigidbody.useGravity = false;
            pickupable.Rigidbody.isKinematic = true;
            pickupable.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            pickupable.ItemCollider.enabled = false;
        }

        private void RestoreDropped(IPickupable dropped)
        {
            dropped.Rigidbody.isKinematic = false;
            dropped.Rigidbody.useGravity = true;
            dropped.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            dropped.ItemCollider.enabled = true;
        }
    }
}

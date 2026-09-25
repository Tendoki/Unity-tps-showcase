using UnityEngine;

namespace Game.Interaction.Items
{
    public abstract class EquippableItem : PickupableItem, IEquippable
    {
        protected bool _equipped;
        
        public void OnEquipped()
        {
            _equipped = true;
            
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        public void OnUnequipped()
        {
            _equipped = false;
            
            rb.useGravity = true;
        }
    }
}
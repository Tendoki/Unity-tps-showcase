using UnityEngine;

namespace Game.Interaction.Items
{
    public abstract class WeaponItem : PickupableItem, IWeapon
    {
        [SerializeField] private Transform muzzlePoint;

        public Transform MuzzlePoint => muzzlePoint;

        public abstract void Fire();
    }
}

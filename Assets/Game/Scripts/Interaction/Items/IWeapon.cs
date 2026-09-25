using UnityEngine;

namespace Game.Interaction.Items
{
    public interface IWeapon
    {
        Transform MuzzlePoint { get; }
        void Fire();
    }
}

using UnityEngine;

namespace Game.Interaction.Items
{
    public interface IEquippable
    { 
        public void OnEquipped();
        public void OnUnequipped();
    }
}
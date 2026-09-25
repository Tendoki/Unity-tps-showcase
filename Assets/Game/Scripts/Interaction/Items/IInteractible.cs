using UnityEngine;

namespace Game.Interaction.Items
{
    public interface IInteractible
    {
        public Vector3 Center { get; }
        public Transform Transform { get; }
        public string InteractionText { get; }
        public string DisplayName { get; }

        public bool IsInteractible();
        public void Interact();
        public void HoverEnter();
        public void HoverExit();
    }
}
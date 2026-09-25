using UnityEngine;

namespace Game.Interaction.Items
{
    public abstract class Item : MonoBehaviour, IInteractible
    {
        [SerializeField] protected Collider itemCollider;
        
        protected bool _blockInteraction;

        public Vector3 Center => transform.position;
        public Transform Transform => transform;
        public string InteractionText { get; }
        public string DisplayName { get; }
        
        public abstract bool IsInteractible();

        public abstract void Interact();

        public abstract void HoverEnter();

        public abstract void HoverExit();
    }
}
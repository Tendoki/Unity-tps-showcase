using Game.Player.ThirdPerson.Interaction;
using Game.Interaction.Items;
using Game.Scripts.Services.Input;
using Game.Services;
using UnityEngine;

namespace Game.Player.Interaction
{
    public class CharacterInteractionHandler : MonoBehaviour
    {
        [SerializeField] private TPHeldItemController heldItemController;
        [SerializeField] private InteractionRaycaster interactionRaycaster;

        private IInputService _inputService;
        
        private void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
        }
        
        public void Tick()
        {
            if (_inputService.InteractPressedThisFrame)
            {
                TryInteract(interactionRaycaster.CurrentHovered);
            }
            else if (_inputService.DropPressedThisFrame)
            {
                heldItemController.DropCurrent();
            }
        }
        
        public void TryInteract(IInteractible target)
        {
            if (target == null)
                return;
            
            if (target is IPickupable pickupable)
                heldItemController.TryPickup(pickupable);
            else
                target.Interact();
        }
    }
}

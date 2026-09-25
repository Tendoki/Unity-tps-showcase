using System;
using Game.Player;
using Game.Player.ThirdPerson.Movement;
using Game.Player.ThirdPerson.Interaction;
using Game.Player.ThirdPerson.Camera;
using Game.Player.ThirdPerson.Animation;
using Game.Player.Interaction;
using Game.Interaction.Items;
using Game.Scripts.Services.Input;
using Game.Services;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Player.ThirdPerson
{
    public class TPPlayerController : MonoBehaviour, ICharacterController
    {
        [SerializeField] private TPLocomotionController locomotionController;
        [SerializeField] private TPCharacterAnimationController animationController;
        [SerializeField] private TPCameraController cameraController;
        [SerializeField] private InteractionRaycaster interactionRaycaster;
        [SerializeField] private CharacterInteractionHandler interactionHandler;
        [SerializeField] private TPHeldItemController heldItemController;

        private IInputService _inputService;
        
        private bool _isAiming;

        public bool IsAiming => _isAiming;

        private void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
        }

        private void Update()
        {
            TickInput();
            cameraController.ReadLookInput();
            UpdateAimingState();
            TickMovement();
            cameraController.ApplyLookTransform();
            TickPresentation();
            TickInteraction();
        }

        private void FixedUpdate()
        {
            TickMovementPhysics();
            ResetTransientInput();
            TickHeldItemPhysics();
        }

        private void TickInput()
        {
            _inputService.Tick();
        }

        private void TickMovement()
        {
            locomotionController.Tick();
        }

        private void TickPresentation()
        {
            animationController.Tick();
            cameraController.SetAimCameraActive(_isAiming);
        }

        private void TickInteraction()
        {
            interactionRaycaster.Tick();
            interactionHandler.Tick();
        }

        private void TickMovementPhysics()
        {
            locomotionController.FixedTick();
        }

        private void TickHeldItemPhysics()
        {
            heldItemController.FixedTick();
        }

        private void ResetTransientInput()
        {
            _inputService.ResetTransientState();
        }

        private void UpdateAimingState()
        {
            if (heldItemController.CurrentPickup is not IWeapon)
            {
                _isAiming = false;
                return;
            }

            if (_inputService.AimPressedThisFrame)
            {
                if (_isAiming)
                {
                    _isAiming = false;
                    return;
                }

                if (!locomotionController.IsFalling && !locomotionController.IsCrouching)
                {
                    _isAiming = true;
                }
            }
        }
    }
}

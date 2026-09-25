using UnityEngine;

namespace Game.Player.ThirdPerson.Movement
{
    [RequireComponent(typeof(PlayerCapsuleStanceController))]
    public class PlayerCrouchController : MonoBehaviour
    {
        [SerializeField] private PlayerCapsuleStanceController capsuleStance;
        [SerializeField] private float minCrouchTime = 0.2f;

        private float _minCrouchTimer;

        public bool IsCrouching { get; private set; }

        private void Awake()
        {
            if (capsuleStance == null)
            {
                capsuleStance = GetComponent<PlayerCapsuleStanceController>();
            }
        }

        public void Tick(bool wantsCrouch, bool canEnterCrouch, bool isAiming, float deltaTime)
        {
            if (canEnterCrouch && wantsCrouch && !IsCrouching && !isAiming)
            {
                EnterCrouch();
            }

            if (IsCrouching)
            {
                UpdateExit(wantsCrouch, deltaTime);
            }

            capsuleStance.Tick(IsCrouching, deltaTime);
        }

        private void EnterCrouch()
        {
            IsCrouching = true;
            _minCrouchTimer = minCrouchTime;
        }

        private void UpdateExit(bool wantsCrouch, float deltaTime)
        {
            _minCrouchTimer -= deltaTime;

            if (!wantsCrouch && _minCrouchTimer <= 0f && capsuleStance.CanStand())
            {
                IsCrouching = false;
            }
        }
    }
}

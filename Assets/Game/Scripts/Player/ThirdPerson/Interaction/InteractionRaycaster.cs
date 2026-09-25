using Game.Player;
using Game.Interaction.Items;
using UnityEngine;

namespace Game.Player.ThirdPerson.Interaction
{
    public class InteractionRaycaster : MonoBehaviour
    {
        [SerializeField] private Transform origin;
        [SerializeField] private LayerMask interactionMask;
        [SerializeField] private float forwardOffset = 1f;
        [SerializeField] private float radius = 0.75f;

        [Header("Debug")]
        [SerializeField] private bool drawDebug = true;
        [SerializeField] private Color sphereColor = Color.yellow;

        private readonly Collider[] _hits = new Collider[32];
        private Vector3 _lastSphereCenter;

        public IInteractible CurrentHovered { get; private set; }

        public void Tick()
        {
            UpdateHovered();
        }

        private void UpdateHovered()
        {
            IInteractible nextHovered = FindInteractible();

            if (CurrentHovered == nextHovered)
                return;

            CurrentHovered?.HoverExit();
            CurrentHovered = nextHovered;
            CurrentHovered?.HoverEnter();
        }

        private IInteractible FindInteractible()
        {
            Vector3 sphereCenter = GetSphereCenter();
            _lastSphereCenter = sphereCenter;

            int hitCount = Physics.OverlapSphereNonAlloc(
                sphereCenter,
                radius,
                _hits,
                interactionMask,
                QueryTriggerInteraction.Ignore
            );

            float closestDistance = float.PositiveInfinity;
            IInteractible closest = null;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hits[i];

                if (hit.transform.IsChildOf(transform))
                    continue;

                IInteractible interactible = hit.GetComponentInParent<IInteractible>();

                if (interactible == null)
                    continue;

                if (!interactible.IsInteractible())
                    continue;

                float distanceToCenter = (interactible.Transform.position - sphereCenter).sqrMagnitude;

                if (distanceToCenter >= closestDistance)
                    continue;

                closestDistance = distanceToCenter;
                closest = interactible;
            }

            return closest;
        }

        private Vector3 GetSphereCenter()
        {
            return origin.position + origin.forward * forwardOffset + origin.up * radius;
        }

        private void OnDrawGizmos()
        {
            if (!drawDebug)
                return;

            Gizmos.color = sphereColor;
            Gizmos.DrawWireSphere(Application.isPlaying ? _lastSphereCenter : GetSphereCenter(), radius);
        }
    }
}

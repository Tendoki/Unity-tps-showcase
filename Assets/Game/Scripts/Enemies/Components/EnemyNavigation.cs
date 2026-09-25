using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemies.Components
{
    public class EnemyNavigation : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private float samplePositionRadius = 1f;

        public Vector3 DesiredVelocity => agent.desiredVelocity;
        public Vector3 SteeringTarget => agent.steeringTarget;
        public float RemainingDistance => agent.remainingDistance;
        public NavMeshPathStatus PathStatus => agent.pathStatus;
        public bool HasValidPath => agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete;

        private void Awake()
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.autoTraverseOffMeshLink = false;
        }

        public bool SetDestination(Vector3 destination)
        {
            if (!agent.isOnNavMesh)
            {
                return false;
            }

            return agent.SetDestination(destination);
        }

        public void SyncAgentPosition(Vector3 position)
        {
            if (!agent.isOnNavMesh)
            {
                return;
            }

            agent.nextPosition = position;
        }

        public bool TrySamplePosition(Vector3 position, out Vector3 sampledPosition)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, samplePositionRadius, agent.areaMask))
            {
                sampledPosition = hit.position;
                return true;
            }

            sampledPosition = position;
            return false;
        }

        public bool CanReach(Vector3 destination)
        {
            if (!agent.isOnNavMesh)
            {
                return false;
            }

            NavMeshPath path = new();
            return NavMesh.CalculatePath(transform.position, destination, agent.areaMask, path)
                   && path.status == NavMeshPathStatus.PathComplete;
        }

        public bool TryFindReachablePointNear(
            Vector3 center,
            float searchRadius,
            int sampleCount,
            out Vector3 destination)
        {
            destination = center;

            if (!agent.isOnNavMesh)
            {
                return false;
            }

            float bestSqrDistance = float.PositiveInfinity;
            bool hasDestination = false;

            if (TryEvaluateReachablePoint(center, center, ref bestSqrDistance, ref destination))
            {
                hasDestination = true;
            }

            int clampedSampleCount = Mathf.Max(1, sampleCount);
            int ringCount = Mathf.Max(1, Mathf.CeilToInt(searchRadius));

            for (int ring = 1; ring <= ringCount; ring++)
            {
                float radius = searchRadius * ring / ringCount;

                for (int i = 0; i < clampedSampleCount; i++)
                {
                    float angle = i * Mathf.PI * 2f / clampedSampleCount;
                    Vector3 offset = new(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                    Vector3 candidate = center + offset;

                    if (TryEvaluateReachablePoint(candidate, center, ref bestSqrDistance, ref destination))
                    {
                        hasDestination = true;
                    }
                }
            }

            return hasDestination;
        }

        public bool HasReachedDestination(float stoppingDistance)
        {
            return !agent.pathPending
                   && agent.hasPath
                   && agent.remainingDistance <= stoppingDistance;
        }

        private bool TryEvaluateReachablePoint(
            Vector3 candidate,
            Vector3 center,
            ref float bestSqrDistance,
            ref Vector3 bestDestination)
        {
            if (!TrySamplePosition(candidate, out Vector3 sampledPosition))
            {
                return false;
            }

            NavMeshPath path = new();

            if (!NavMesh.CalculatePath(transform.position, sampledPosition, agent.areaMask, path)
                || path.status != NavMeshPathStatus.PathComplete)
            {
                return false;
            }

            float sqrDistance = (sampledPosition - center).sqrMagnitude;

            if (sqrDistance >= bestSqrDistance)
            {
                return false;
            }

            bestSqrDistance = sqrDistance;
            bestDestination = sampledPosition;
            return true;
        }
    }
}

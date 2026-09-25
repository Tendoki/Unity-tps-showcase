using UnityEngine;

namespace Game.Enemies.Components
{
    public abstract class EnemyTargetDetector : MonoBehaviour
    {
        public abstract Transform CurrentTarget { get; }
        public abstract bool HasTarget { get; }
        public abstract float DistanceToTarget { get; }
        public abstract bool HasLineOfSightToTarget { get; }

        public abstract bool TryDetectTarget();
        public abstract bool RefreshTarget();
        public abstract void ClearTarget();
    }
}

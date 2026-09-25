using Game.Enemies;
using UnityEngine;

namespace Game.Enemies.Zombie
{
    public interface IZombieStateController : IEnemyStateController
    {
        float PatrolSpeed { get; }
        float PatrolRadius { get; }
        float PatrolStoppingDistance { get; }
        float MinPatrolWaitTime { get; }
        float MaxPatrolWaitTime { get; }

        float ChaseSpeed { get; }
        float ChaseDestinationSearchRadius { get; }
        int ChaseDestinationSampleCount { get; }
        float PatrolGroundProbeOffsetRatio { get; }
        float ChaseGroundProbeOffsetRatio { get; }
        float AttackDistance { get; }
        float AttackCooldown { get; }
        float MovementRotationSmoothing { get; }
        float AttackRotationSmoothing { get; }
        bool UseRootMotionLocomotion { get; }

        Vector3 PatrolCenter { get; }
        Vector3 LastKnownTargetPosition { get; }
        bool IsAttackReady { get; }
        bool HasLastKnownTargetPosition { get; }

        void StartAttackCooldown();
        void RememberTargetPosition(Vector3 position);
        void ClearLastKnownTargetPosition();
    }
}

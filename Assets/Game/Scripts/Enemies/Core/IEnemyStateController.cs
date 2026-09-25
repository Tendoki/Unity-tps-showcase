using Game.Enemies.Components;
using UnityEngine;

namespace Game.Enemies
{
    public interface IEnemyStateController
    {
        Transform EnemyTransform { get; }
        EnemyTargetDetector TargetDetector { get; }
        EnemyNavigation Navigation { get; }
        EnemyLocomotionController Locomotion { get; }
        EnemyAnimationController Animation { get; }

        bool IsDead { get; }
        bool WasHit { get; }

        void ConsumeHit();
    }
}

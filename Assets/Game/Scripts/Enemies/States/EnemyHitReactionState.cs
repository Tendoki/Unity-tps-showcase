using Game.Enemies;
namespace Game.Enemies.States
{
    public class EnemyHitReactionState : EnemyState
    {
        public EnemyHitReactionState(IEnemyStateController enemy) : base(enemy)
        {
        }

        public override void OnEnter()
        {
            Enemy.ConsumeHit();
            Enemy.Animation.PlayHitReaction();
        }

        public override void FixedUpdate()
        {
            Enemy.Animation.DiscardRootMotion();
            Enemy.Locomotion.Stop();
        }
    }
}

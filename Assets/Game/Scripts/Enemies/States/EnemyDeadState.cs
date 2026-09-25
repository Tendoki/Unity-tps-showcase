using Game.Enemies;
namespace Game.Enemies.States
{
    public class EnemyDeadState : EnemyState
    {
        public EnemyDeadState(IEnemyStateController enemy) : base(enemy)
        {
        }

        public override void OnEnter()
        {
            Enemy.Animation.PlayDeath();
        }

        public override void FixedUpdate()
        {
            Enemy.Animation.DiscardRootMotion();
            Enemy.Locomotion.Stop();
        }
    }
}

using Game.Enemies;
using Game.FSM;
namespace Game.Enemies.States
{
    public abstract class EnemyState : BaseState
    {
        protected readonly IEnemyStateController Enemy;

        protected EnemyState(IEnemyStateController enemy)
        {
            Enemy = enemy;
        }
    }
}

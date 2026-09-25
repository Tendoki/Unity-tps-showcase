using UnityEngine;

namespace Game.Actors.Movement
{
    public interface IActorMotorModifier
    {
        void BeforeMove(float deltaTime);
        void ModifyMomentum(ref Vector3 momentum, ActorMotor motor, float deltaTime);
        void AfterMove();
    }
}

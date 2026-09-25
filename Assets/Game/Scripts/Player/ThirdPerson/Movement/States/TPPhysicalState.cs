using Game.FSM;

namespace Game.Player.ThirdPerson.Movement
{
    abstract class TPPhysicalState : BaseState
    {
        protected readonly ITPLocomotionStateActions Locomotion;

        protected TPPhysicalState(ITPLocomotionStateActions locomotion)
        {
            Locomotion = locomotion;
        }
    }
}

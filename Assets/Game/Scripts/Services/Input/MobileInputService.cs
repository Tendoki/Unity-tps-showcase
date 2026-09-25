
namespace Game.Scripts.Services.Input
{
    public class MobileInputService : InputService
    {
        private const float AutoSprintThreshold = 0.9f;

        public override void Tick()
        {
            base.Tick();

            LookInput = MobileLookArea.ConsumeLookDelta();
            SprintHeld = MoveInput.sqrMagnitude >= AutoSprintThreshold * AutoSprintThreshold;
        }
    }
}

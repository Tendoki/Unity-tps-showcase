namespace Game.Player.ThirdPerson.Movement
{
    public interface ITPLocomotionStateActions
    {
        void UpdateGroundedControls();
        void UpdateAirborneControls(bool canEnterCrouch);
        void SimulateGroundedMovement();
        void SimulateAirborneMovement();
        void SimulateFallingMovement();
        bool TryStartJump();
    }
}

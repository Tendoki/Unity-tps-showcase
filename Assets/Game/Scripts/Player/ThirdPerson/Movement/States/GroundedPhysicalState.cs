namespace Game.Player.ThirdPerson.Movement
{
    class GroundedPhysicalState : TPPhysicalState
    {
        public GroundedPhysicalState(ITPLocomotionStateActions locomotion) : base(locomotion)
        {
        }

        public override void Update()
        {
            Locomotion.UpdateGroundedControls();
        }

        public override void FixedUpdate()
        {
            Locomotion.SimulateGroundedMovement();
        }
    }
}

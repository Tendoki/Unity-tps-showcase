namespace Game.Player.ThirdPerson.Movement
{
    class CoyotePhysicalState : TPPhysicalState
    {
        public CoyotePhysicalState(ITPLocomotionStateActions locomotion) : base(locomotion)
        {
        }

        public override void Update()
        {
            Locomotion.UpdateAirborneControls(canEnterCrouch: false);
        }

        public override void FixedUpdate()
        {
            Locomotion.SimulateAirborneMovement();
        }
    }
}

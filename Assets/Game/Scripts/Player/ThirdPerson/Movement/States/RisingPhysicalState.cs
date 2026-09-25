namespace Game.Player.ThirdPerson.Movement
{
    class RisingPhysicalState : TPPhysicalState
    {
        public RisingPhysicalState(ITPLocomotionStateActions locomotion) : base(locomotion)
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

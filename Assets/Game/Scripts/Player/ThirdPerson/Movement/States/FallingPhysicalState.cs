namespace Game.Player.ThirdPerson.Movement
{
    class FallingPhysicalState : TPPhysicalState
    {
        public FallingPhysicalState(ITPLocomotionStateActions locomotion) : base(locomotion)
        {
        }

        public override void Update()
        {
            Locomotion.UpdateAirborneControls(canEnterCrouch: false);
        }

        public override void FixedUpdate()
        {
            Locomotion.SimulateFallingMovement();
        }
    }
}

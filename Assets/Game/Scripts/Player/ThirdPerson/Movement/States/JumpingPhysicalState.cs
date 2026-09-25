namespace Game.Player.ThirdPerson.Movement
{
    class JumpingPhysicalState : TPPhysicalState
    {
        public JumpingPhysicalState(ITPLocomotionStateActions locomotion) : base(locomotion)
        {
        }

        public override void OnEnter()
        {
            Locomotion.TryStartJump();
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

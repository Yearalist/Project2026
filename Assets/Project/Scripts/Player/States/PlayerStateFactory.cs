

namespace ToySiege.Player.States
{
    public class PlayerStateFactory
    {
        private readonly PlayerController _ctx;

        public PlayerStateFactory(PlayerController ctx)
        {
            _ctx = ctx;
        }

        public PlayerIdleState Idle() => new(_ctx, this);
        public PlayerWalkState Walk() => new(_ctx, this);       
        public PlayerSprintState Sprint() => new(_ctx, this);   
        public PlayerJumpState Jump() => new(_ctx, this);
        public PlayerDoubleJumpState DoubleJump() => new(_ctx, this);
        public PlayerSlideState Slide() => new(_ctx, this);
    }
}
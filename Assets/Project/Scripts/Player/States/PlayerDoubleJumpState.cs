using UnityEngine;

namespace ToySiege.Player.States
{
    public class PlayerDoubleJumpState : PlayerBaseState
    {
        public PlayerDoubleJumpState(PlayerController ctx, PlayerStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Debug.Log("<color=magenta>→ STATE: Double Jump</color>");
            Ctx.Anim.TriggerDoubleJump();

            // İkinci zıplama kuvveti
            Ctx.SetVerticalVelocity(Ctx.Config.DoubleJumpForce);

            // Hakkı tüket — artık havada bir daha zıplayamaz
            Ctx.ConsumeDoubleJump();
        }

        public override void Execute()
        {
            base.Execute();
            Ctx.Anim.SetVerticalSpeed(Ctx.VerticalVelocity);
        }

        public override void FixedExecute()
        {
            Ctx.HandleAirMovement();
            Ctx.ApplyGravity();
            Ctx.MoveCharacter();
        }

        protected override void CheckTransitions()
        {
            // Tek çıkış: yere iniş
            // Double jump'tan sonra tekrar zıplama YOK (GDD kuralı)
            if (Ctx.IsGrounded && Ctx.VerticalVelocity <= 0f)
            {
                var nextState = Ctx.Input.MoveInput.sqrMagnitude > 0.01f
                    ? (PlayerBaseState)Factory.Run()
                    : Factory.Idle();
                Ctx.FSM.ChangeState(nextState);
            }
        }
    }
}
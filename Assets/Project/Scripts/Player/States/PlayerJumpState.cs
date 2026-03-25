using UnityEngine;

namespace ToySiege.Player.States
{
    public class PlayerJumpState : PlayerBaseState
    {
        public PlayerJumpState(PlayerController ctx, PlayerStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Debug.Log("<color=cyan>→ STATE: Jump</color>");
            Ctx.Anim.TriggerJump();

            // Yukarı kuvvet uygula
            Ctx.SetVerticalVelocity(Ctx.Config.JumpForce);

            // Double jump hakkını AÇ
            Ctx.ResetDoubleJump();
        }

        public override void Execute()
        {
            base.Execute(); // CheckTransitions
            Ctx.Anim.SetVerticalSpeed(Ctx.VerticalVelocity);
        }

        public override void FixedExecute()
        {
            // Havada yönlendirme (biraz daha az kontrol)
            Ctx.HandleAirMovement();
            Ctx.ApplyGravity();
            Ctx.MoveCharacter();
        }

        protected override void CheckTransitions()
        {
            // 1. Double Jump (GDD: "2. zıplama sadece 1 kere yapılabilir")
            if (Ctx.Input.JumpPressed && Ctx.HasDoubleJump)
            {
                Ctx.FSM.ChangeState(Factory.DoubleJump());
                return;
            }

            // 2. Yere iniş — aşağı hareket ediyorken ve yere değmişse
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
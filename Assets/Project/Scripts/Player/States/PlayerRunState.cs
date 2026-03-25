using UnityEngine;

namespace ToySiege.Player.States
{
    public class PlayerRunState : PlayerBaseState
    {
        public PlayerRunState(PlayerController ctx, PlayerStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Debug.Log("<color=white>→ STATE: Run</color>");
            Ctx.Anim.SetRunning(true);
        }

        public override void Execute()
        {
            base.Execute(); // CheckTransitions çağrılır
        }

        public override void FixedExecute()
        {
            // Kamera yönüne göre hareket hesapla ve uygula
            Ctx.HandleGroundMovement();
            Ctx.ApplyGravity();
            Ctx.MoveCharacter();
        }

        public override void Exit()
        {
            Ctx.Anim.SetRunning(false);
        }

        protected override void CheckTransitions()
        {
            // 1. Zıplama
            if (Ctx.Input.JumpPressed && Ctx.IsGrounded)
            {
                Ctx.FSM.ChangeState(Factory.Jump());
                return;
            }

            // 2. Slide (GDD: "Karakter dururken değil, hareket halindeyken slide yapacak")
            if (Ctx.Input.SlidePressed && Ctx.CanSlide)
            {
                Ctx.FSM.ChangeState(Factory.Slide());
                return;
            }

            // 3. Durma → Idle
            if (Ctx.Input.MoveInput.sqrMagnitude < 0.01f)
            {
                Ctx.FSM.ChangeState(Factory.Idle());
            }
        }
    }
}
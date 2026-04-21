using UnityEngine;
using ToySiege.Core.FSM;

namespace ToySiege.Player.States
{
    public class PlayerJumpState : PlayerBaseState
    {
        public PlayerJumpState(PlayerController ctx, PlayerStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Debug.Log("<color=cyan>→ STATE: Jump</color>");
            Ctx.Anim.TriggerJump();              // ← TriggerDoubleJump DEĞİL
            Ctx.SetVerticalVelocity(Ctx.Config.JumpForce);
            Ctx.ResetDoubleJump();
            Ctx.VFX.StopFootDust();
            GameFeelManager.Instance?.OnJump();
        }

        public override void Execute()
        {
            base.Execute();
            // Bu değer Animator'da Jumping Up → Falling Idle geçişini tetikler
            Ctx.Anim.SetVerticalSpeed(Ctx.VerticalVelocity);
        }

        public override void FixedExecute()
        {
            Ctx.HandleMouseRotation();
            Ctx.HandleAirMovement();
            Ctx.ApplyGravity();
            Ctx.MoveCharacter();
        }

        protected override void CheckTransitions()
        {
            // Önce double jump kontrolü
            if (Ctx.Input.JumpPressed && Ctx.HasDoubleJump)
            {
                Ctx.FSM.ChangeState(Factory.DoubleJump());
                return;
            }

            // Yere iniş — ÖNCE Landing trigger'ı at, SONRA state değiştir
            if (Ctx.IsGrounded && Ctx.VerticalVelocity <= 0f)
            {
                Ctx.Anim.TriggerLanding();  // Animator: Falling Idle → Falling To Landing
                GameFeelManager.Instance?.OnLanding(Ctx.VerticalVelocity);

                if (Ctx.Input.MoveInput.sqrMagnitude > 0.01f)
                {
                    if (Ctx.Input.SprintHeld)
                        Ctx.FSM.ChangeState(Factory.Sprint());
                    else
                        Ctx.FSM.ChangeState(Factory.Walk());
                }
                else
                    Ctx.FSM.ChangeState(Factory.Idle());
            }
        }
    }
}

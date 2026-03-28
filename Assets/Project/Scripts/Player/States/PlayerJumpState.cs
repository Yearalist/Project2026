

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
            Ctx.Anim.TriggerJump();
            Ctx.SetVerticalVelocity(Ctx.Config.JumpForce);
            Ctx.ResetDoubleJump();
            Ctx.VFX.StopFootDust();      // YENİ — havada toz yok
        }

        public override void Execute()
        {
            base.Execute();
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
            if (Ctx.Input.JumpPressed && Ctx.HasDoubleJump)
            { Ctx.FSM.ChangeState(Factory.DoubleJump()); return; }

            if (Ctx.IsGrounded && Ctx.VerticalVelocity <= 0f)
            {
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
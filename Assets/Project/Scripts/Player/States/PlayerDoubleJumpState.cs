using UnityEngine;
using ToySiege.Core.FSM;

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
            Ctx.SetVerticalVelocity(Ctx.Config.DoubleJumpForce);
            Ctx.ConsumeDoubleJump();
            Ctx.VFX.StopFootDust();
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
            if (Ctx.IsGrounded && Ctx.VerticalVelocity <= 0f)
            {
                Ctx.Anim.TriggerLanding();

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

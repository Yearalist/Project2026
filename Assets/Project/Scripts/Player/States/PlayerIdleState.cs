using UnityEngine;
using ToySiege.Core.FSM;

namespace ToySiege.Player.States
{
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerController ctx, PlayerStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Ctx.Anim.SetIdle();
            Ctx.SetHorizontalVelocity(Vector3.zero);
            Ctx.IsSprinting = false;
            Ctx.VFX.StopFootDust();
        }

        public override void FixedExecute()
        {
            Ctx.HandleMouseRotation();
            Ctx.ApplyGravity();
            Ctx.MoveCharacter();
            // Idle'dayken de Blend Tree'yi besle (smooth 0'a iner)
            Ctx.Anim.UpdateLocomotion(Ctx.Input.MoveInput, false);
        }

        protected override void CheckTransitions()
        {
            if (Ctx.Input.JumpPressed && Ctx.IsGrounded)
            {
                Ctx.FSM.ChangeState(Factory.Jump());
                return;
            }

            if (Ctx.Input.MoveInput.sqrMagnitude > 0.01f)
            {
                if (Ctx.Input.SprintHeld)
                    Ctx.FSM.ChangeState(Factory.Sprint());
                else
                    Ctx.FSM.ChangeState(Factory.Walk());
            }
        }
    }
}
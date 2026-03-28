

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
            Debug.Log("<color=white>→ STATE: Idle</color>");
            Ctx.Anim.SetIdle();
            Ctx.SetHorizontalVelocity(Vector3.zero);
            Ctx.IsSprinting = false;
            Ctx.VFX.StopFootDust();    // YENİ — dururken toz yok
        }

        public override void FixedExecute()
        {
            Ctx.HandleMouseRotation();
            Ctx.ApplyGravity();
            Ctx.MoveCharacter();
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
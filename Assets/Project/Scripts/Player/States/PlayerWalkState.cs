

using UnityEngine;
using ToySiege.Core.FSM;

namespace ToySiege.Player.States
{
    public class PlayerWalkState : PlayerBaseState
    {
        public PlayerWalkState(PlayerController ctx, PlayerStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Debug.Log("<color=white>→ STATE: Walk</color>");
            Ctx.Anim.SetRunning(true);
            Ctx.IsSprinting = false;
            Ctx.VFX.StartWalkDust();    
        }

        public override void FixedExecute()
        {
            Ctx.HandleMouseRotation();
            Ctx.HandleWalkMovement();
            Ctx.ApplyGravity();
            Ctx.MoveCharacter();
        }

        public override void Exit()
        {
            Ctx.Anim.SetRunning(false);
            Ctx.VFX.StopFootDust();     
        }

        protected override void CheckTransitions()
        {
            if (Ctx.Input.JumpPressed && Ctx.IsGrounded)
            { Ctx.FSM.ChangeState(Factory.Jump()); return; }

            if (Ctx.Input.SprintHeld)
            { Ctx.FSM.ChangeState(Factory.Sprint()); return; }

            if (Ctx.Input.MoveInput.sqrMagnitude < 0.01f)
            { Ctx.FSM.ChangeState(Factory.Idle()); }
        }
    }
}
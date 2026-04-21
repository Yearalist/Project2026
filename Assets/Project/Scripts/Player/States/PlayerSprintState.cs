using UnityEngine;
using ToySiege.Core.FSM;

namespace ToySiege.Player.States
{
    public class PlayerSprintState : PlayerBaseState
    {
        public PlayerSprintState(PlayerController ctx, PlayerStateFactory factory)
            : base(ctx, factory) { }

      

        public override void Execute()
        {
            base.Execute();
        }

        public override void Enter()
        {
            Ctx.IsSprinting = true;
            Ctx.Anim.SetSprinting(true);
            Ctx.VFX.StartSprintDust();
            GameFeelManager.Instance?.OnSprintStart();
        }

        public override void FixedExecute()
        {
            Ctx.HandleMouseRotation();
            Ctx.HandleSprintMovement();
            Ctx.ApplyGravity();
            Ctx.MoveCharacter();
            Ctx.Anim.UpdateLocomotion(Ctx.Input.MoveInput, true);
        }

        public override void Exit()
        {
            Ctx.Anim.SetSprinting(false);
            Ctx.VFX.StopFootDust();
            GameFeelManager.Instance?.OnSprintEnd();
        }

        protected override void CheckTransitions()
        {
            if (Ctx.Input.JumpPressed && Ctx.IsGrounded)
            { Ctx.FSM.ChangeState(Factory.Jump()); return; }

            if (Ctx.Input.SlidePressed && Ctx.CanSlide)
            { Ctx.FSM.ChangeState(Factory.Slide()); return; }

            if (!Ctx.Input.SprintHeld)
            { Ctx.FSM.ChangeState(Factory.Walk()); return; }

            if (Ctx.Input.MoveInput.sqrMagnitude < 0.01f)
            { Ctx.FSM.ChangeState(Factory.Idle()); }
        }
    }
}
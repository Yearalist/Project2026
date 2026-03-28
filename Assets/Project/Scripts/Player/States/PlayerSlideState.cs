

using UnityEngine;
using ToySiege.Core.FSM;

namespace ToySiege.Player.States
{
    public class PlayerSlideState : PlayerBaseState
    {
        private float _timer;
        private Vector3 _slideDirection;

        public PlayerSlideState(PlayerController ctx, PlayerStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Debug.Log("<color=green>→ STATE: Slide</color>");
            Ctx.Anim.TriggerSlide();

            _slideDirection = Ctx.transform.forward;
            _timer = Ctx.Config.SlideDuration;

            Ctx.SetColliderHeight(
                Ctx.Config.SlideColliderHeight,
                Ctx.Config.SlideColliderCenterY
            );

            // YENİ — Slide hız patlaması + iz efekti
            Ctx.VFX.PlaySlideBurst();
        }

        public override void Execute()
        {
            _timer -= Time.deltaTime;
            base.Execute();
        }

        public override void FixedExecute()
        {
            Ctx.HandleMouseRotation();
            Vector3 velocity = _slideDirection * Ctx.Config.SlideSpeed;
            Ctx.SetHorizontalVelocity(velocity);
            Ctx.ApplyGravity();
            Ctx.MoveCharacter();
        }

        public override void Exit()
        {
            Ctx.SetColliderHeight(
                Ctx.Config.NormalColliderHeight,
                Ctx.Config.NormalColliderCenterY
            );
            Ctx.StartSlideCooldown();

            // YENİ — Slide iz efektini durdur
            Ctx.VFX.StopSlideTrail();
        }

        protected override void CheckTransitions()
        {
            if (_timer > 0f) return;

            if (Ctx.Input.MoveInput.sqrMagnitude > 0.01f)
            {
                if (Ctx.Input.SprintHeld)
                    Ctx.FSM.ChangeState(Factory.Sprint());
                else
                    Ctx.FSM.ChangeState(Factory.Walk());
            }
            else
            {
                Ctx.FSM.ChangeState(Factory.Idle());
            }
        }
    }
}
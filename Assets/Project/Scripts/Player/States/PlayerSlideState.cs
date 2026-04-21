using UnityEngine;
using ToySiege.Core.FSM;

namespace ToySiege.Player.States
{
    public class PlayerSlideState : PlayerBaseState
    {
        private float _timer;
        private float _duration;
        private Vector3 _slideDirection;

        public PlayerSlideState(PlayerController ctx, PlayerStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Debug.Log("<color=green>→ STATE: Slide</color>");
            _timer = Ctx.Config.SlideDuration;
            _slideDirection = Ctx.transform.forward;

            Ctx.Anim.TriggerSlide();
            Ctx.VFX.PlaySlideBurst();

            // Collider'ı küçült (Hazır yazdığın metodu kullanalım!)
            Ctx.SetColliderHeight(Ctx.Config.SlideColliderHeight, Ctx.Config.SlideColliderCenterY);
            GameFeelManager.Instance?.OnSlideStart();
        }

        public override void FixedExecute()
        {
            Ctx.HandleMouseRotation();

            // Sprint hızıyla ilerle — kamera geçişi yumuşak olur
            Vector3 velocity = _slideDirection * Ctx.Config.SprintSpeed;
            Ctx.SetHorizontalVelocity(velocity);
            Ctx.ApplyGravity();
            Ctx.MoveCharacter();
        }

        public override void Execute()
        {
            _timer -= Time.deltaTime;
            base.Execute();
        }

        public override void Exit()
        {
            Ctx.StartSlideCooldown();
            Ctx.VFX.StopSlideTrail();

            // Collider'ı eski haline getir
            Ctx.SetColliderHeight(Ctx.Config.NormalColliderHeight, Ctx.Config.NormalColliderCenterY);
            GameFeelManager.Instance?.OnSlideEnd();
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
                Ctx.FSM.ChangeState(Factory.Idle());
        }
    }
}
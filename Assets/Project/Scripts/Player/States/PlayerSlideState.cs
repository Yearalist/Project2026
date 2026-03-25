using UnityEngine;

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
        }

        public override void Execute()
        {
            // Base class zaten CheckTransitions()'ı burada otomatik çağırıyor, 
            // sadece timer'ı düşmemiz yeterli.
            _timer -= Time.deltaTime;

            // Base.Execute() çağırırsan CheckTransitions() otomatik çalışır 
            // (Base class koduna göre öyle görünüyor)
            base.Execute();
        }

        // İŞTE EKSİK OLAN VE HATAYA SEBEP OLAN KISIM BURASI:
        protected override void CheckTransitions()
        {
            if (_timer <= 0f)
            {
                var nextState = Ctx.Input.MoveInput.sqrMagnitude > 0.01f
                    ? (PlayerBaseState)Factory.Run()
                    : Factory.Idle();

                Ctx.FSM.ChangeState(nextState);
            }
        }

        public override void FixedExecute()
        {
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
        }
    }
}
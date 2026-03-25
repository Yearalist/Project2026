using UnityEngine;

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
            // Yatay hızı sıfırla (yavaşça durma)
            Ctx.SetHorizontalVelocity(Vector3.zero);
        }

        public override void FixedExecute()
        {
            Ctx.ApplyGravity();
            Ctx.MoveCharacter();
        }

        protected override void CheckTransitions()
        {
            // Öncelik sırası önemli!

            // 1. Zıplama kontrolü
            if (Ctx.Input.JumpPressed && Ctx.IsGrounded)
            {
                Ctx.FSM.ChangeState(Factory.Jump());
                return;
            }

            // 2. Hareket kontrolü
            if (Ctx.Input.MoveInput.sqrMagnitude > 0.01f)
            {
                Ctx.FSM.ChangeState(Factory.Run());
            }
        }
    }
}

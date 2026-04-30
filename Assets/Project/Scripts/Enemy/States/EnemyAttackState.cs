using UnityEngine;

namespace ToySiege.Enemy.States
{
    public class EnemyAttackState : EnemyBaseState
    {
        private float _timer;

        public EnemyAttackState(EnemyController ctx, EnemyStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Debug.Log("<color=red>→ ENEMY: Attack!</color>");
            Ctx.StopMoving();
            Ctx.Anim.TriggerAttack();
            Ctx.LookAtTarget();
            _timer = Ctx.Config.AttackDuration;

            if (Ctx.Detection.IsInAttackRange)
            {
                var playerHealth = Ctx.Detection.Target
                    .GetComponent<ToySiege.Player.Health.PlayerHealth>();

                if (playerHealth != null)
                    playerHealth.TakeDamage(Ctx.Config.AttackDamage);
            }
        }

        public override void Execute()
        {
            _timer -= Time.deltaTime;
            Ctx.LookAtTarget();
            base.Execute();
        }

        protected override void CheckTransitions()
        {
            if (_timer > 0f) return;

            Ctx.AttackCooldownTimer = Ctx.Config.AttackCooldown;

            if (Ctx.Detection.IsInAttackRange)
                Ctx.FSM.ChangeState(Factory.Chase());
            else if (Ctx.Detection.IsInLoseRange)
                Ctx.FSM.ChangeState(Factory.Chase());
            else
                Ctx.FSM.ChangeState(Factory.Idle());
        }
    }
}
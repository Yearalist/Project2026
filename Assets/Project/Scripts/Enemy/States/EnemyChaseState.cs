using UnityEngine;

namespace ToySiege.Enemy.States
{
    public class EnemyChaseState : EnemyBaseState
    {
        public EnemyChaseState(EnemyController ctx, EnemyStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Debug.Log("<color=orange>→ ENEMY: Chase</color>");
            Ctx.Anim.SetCombatMode(true);
            // StoppingDistance'ı config'e ayarla (ranged = uzak, melee = yakın)
            Ctx.Agent.stoppingDistance = Ctx.Config.StoppingDistance;
        }

        public override void Execute()
        {
            Ctx.ChaseTarget();
            base.Execute();
        }

        protected override void CheckTransitions()
        {
            // Saldırı menzilinde → Attack
            if (Ctx.Detection.IsInAttackRange && Ctx.CanAttack)
            {
                Ctx.FSM.ChangeState(Factory.Attack());
                return;
            }

            // Oyuncuyu kaybetti → Idle
            if (!Ctx.Detection.IsInLoseRange)
                Ctx.FSM.ChangeState(Factory.Idle());
        }
    }
}
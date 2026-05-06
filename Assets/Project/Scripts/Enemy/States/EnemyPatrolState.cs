using UnityEngine;

namespace ToySiege.Enemy.States
{
    public class EnemyPatrolState : EnemyBaseState
    {
        private Vector3 _patrolTarget;

        public EnemyPatrolState(EnemyController ctx, EnemyStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Debug.Log("<color=grey>→ ENEMY: Patrol</color>");
            Ctx.Agent.speed = Ctx.Config.MoveSpeed;
            Ctx.Anim.SetCombatMode(false);    // ← normal yürüyüş
            _patrolTarget = Ctx.GetRandomPatrolPoint();
            Ctx.Agent.SetDestination(_patrolTarget);
        }

        public override void Execute()
        {
            base.Execute();
        }

        protected override void CheckTransitions()
        {
            // Oyuncuyu görürse → Chase
            if (Ctx.Detection.CanSeeTarget())
            {
                Ctx.FSM.ChangeState(Factory.Chase());
                return;
            }

            // Hedefe vardı → Idle
            if (!Ctx.Agent.pathPending && Ctx.Agent.remainingDistance <= Ctx.Agent.stoppingDistance)
                Ctx.FSM.ChangeState(Factory.Idle());
        }
    }
}
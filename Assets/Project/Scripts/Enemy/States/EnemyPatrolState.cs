using UnityEngine;

namespace ToySiege.Enemy.States
{
    public class EnemyPatrolState : EnemyBaseState
    {
        private Vector3 _patrolTarget;

        public EnemyPatrolState(EnemyController ctx, EnemyStateFactory factory)
            : base(ctx, factory) { }

        // Patrol'da hedefe varma eşiği — stoppingDistance'tan bağımsız
        // (stoppingDistance yüksek olan ranged düşmanlar patrol yapamıyordu)
        private const float PatrolArrivalThreshold = 1.5f;

        public override void Enter()
        {
            Debug.Log("<color=grey>→ ENEMY: Patrol</color>");
            Ctx.Agent.speed = Ctx.Config.MoveSpeed;
            Ctx.Anim.SetCombatMode(false);

            // Patrol sırasında stoppingDistance küçük olmalı (hedefe yürüsün)
            Ctx.Agent.stoppingDistance = 0.5f;

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
                // StoppingDistance'ı config'e geri al (saldırı mesafesi)
                Ctx.Agent.stoppingDistance = Ctx.Config.StoppingDistance;
                Ctx.FSM.ChangeState(Factory.Chase());
                return;
            }

            // Hedefe vardı → Idle (sabit threshold ile, stoppingDistance'tan bağımsız)
            if (!Ctx.Agent.pathPending && Ctx.Agent.remainingDistance <= PatrolArrivalThreshold)
            {
                Ctx.Agent.stoppingDistance = Ctx.Config.StoppingDistance;
                Ctx.FSM.ChangeState(Factory.Idle());
            }
        }
    }
}
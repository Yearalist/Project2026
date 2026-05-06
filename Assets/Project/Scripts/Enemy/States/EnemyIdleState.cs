using UnityEngine;

namespace ToySiege.Enemy.States
{
    public class EnemyIdleState : EnemyBaseState
    {
        private float _waitTimer;

        public EnemyIdleState(EnemyController ctx, EnemyStateFactory factory)
            : base(ctx, factory) { }

        public override void Enter()
        {
            Debug.Log("<color=grey>→ ENEMY: Idle</color>");
            Ctx.StopMoving();
            Ctx.Anim.SetCombatMode(false);    // ← normal idle
            _waitTimer = Random.Range(Ctx.Config.IdleWaitMin, Ctx.Config.IdleWaitMax);
        }

        public override void Execute()
        {
            _waitTimer -= Time.deltaTime;
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

            // Bekleme bitti → Patrol
            if (_waitTimer <= 0f)
                Ctx.FSM.ChangeState(Factory.Patrol());
        }
    }
}
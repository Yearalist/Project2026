using ToySiege.Core.FSM;

namespace ToySiege.Enemy.States
{
    public abstract class EnemyBaseState : IState
    {
        protected readonly EnemyController Ctx;
        protected readonly EnemyStateFactory Factory;

        protected EnemyBaseState(EnemyController ctx, EnemyStateFactory factory)
        {
            Ctx = ctx;
            Factory = factory;
        }

        public virtual void Enter() { }
        public virtual void Execute() { CheckTransitions(); }
        public virtual void FixedExecute() { }
        public virtual void Exit() { }

        protected abstract void CheckTransitions();
    }
}
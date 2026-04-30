namespace ToySiege.Enemy.States
{
    public class EnemyStateFactory
    {
        private readonly EnemyController _ctx;

        public EnemyStateFactory(EnemyController ctx)
        {
            _ctx = ctx;
        }

        public EnemyIdleState Idle() => new(_ctx, this);
        public EnemyPatrolState Patrol() => new(_ctx, this);
        public EnemyChaseState Chase() => new(_ctx, this);
        public EnemyAttackState Attack() => new(_ctx, this);
    }
}
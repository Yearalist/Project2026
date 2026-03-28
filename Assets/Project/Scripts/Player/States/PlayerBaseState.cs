
using ToySiege.Core.FSM;

namespace ToySiege.Player.States
{
    public abstract class PlayerBaseState : IState
    {
        /// <summary>
        /// PlayerController'a erişim. Hareket, fizik, collider, config...
        /// "Ctx" = Context (bağlam) kısaltması.
        /// </summary>
        protected readonly PlayerController Ctx;

        /// <summary>
        /// Diğer state'leri üretmek için fabrika.
        /// Örnek: Factory.Jump() → yeni bir PlayerJumpState döndürür.
        /// </summary>
        protected readonly PlayerStateFactory Factory;

        protected PlayerBaseState(PlayerController ctx, PlayerStateFactory factory)
        {
            Ctx = ctx;
            Factory = factory;
        }

        public virtual void Enter() { }

        public virtual void Execute()
        {
            // Her frame state geçiş kontrolü yap
            CheckTransitions();
        }

        public virtual void FixedExecute() { }

        public virtual void Exit() { }

        /// <summary>
        /// Her state ZORUNLU olarak bunu implemente eder.
        /// "Şu an bu state'deyim ama X olursa Y state'e geçmeliyim"
        /// </summary>
        protected abstract void CheckTransitions();
    }
}
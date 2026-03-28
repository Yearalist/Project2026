

namespace ToySiege.Core.FSM
{
    public interface IState
    {
        /// <summary>
        /// State'e GİRİLDİĞİNDE bir kez çağrılır.
        /// Örnek: Animasyon başlat, değişkenleri sıfırla.
        /// </summary>
        void Enter();

        /// <summary>
        /// Her FRAME çağrılır (Update döngüsü).
        /// Örnek: Input kontrolü, state geçiş kontrolleri.
        /// </summary>
        void Execute();

        /// <summary>
        /// Her FIXED FRAME çağrılır (FixedUpdate döngüsü).
        /// Örnek: Fizik hesaplamaları, CharacterController.Move.
        /// </summary>
        void FixedExecute();

        /// <summary>
        /// State'den ÇIKILIRKEN bir kez çağrılır.
        /// Örnek: Animasyonu durdur, timer'ları temizle.
        /// </summary>
        void Exit();
    }
}
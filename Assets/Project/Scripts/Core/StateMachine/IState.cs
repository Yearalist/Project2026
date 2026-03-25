namespace ToySiege.Core.FSM
{
    public interface IState
    {
        /// <summary>
        /// State'e GÝRÝLDÝÐÝNDE bir kez çaðrýlýr.
        /// Örnek: Animasyon baþlat, deðiþkenleri sýfýrla.
        /// </summary>
        void Enter();

        /// <summary>
        /// Her FRAME çaðrýlýr (Update döngüsü).
        /// Örnek: Input kontrolü, state geçiþ kontrolleri.
        /// </summary>
        void Execute();

        /// <summary>
        /// Her FIXED FRAME çaðrýlýr (FixedUpdate döngüsü).
        /// Örnek: Fizik hesaplamalarý, CharacterController.Move.
        /// </summary>
        void FixedExecute();

        /// <summary>
        /// State'den ÇIKILIRKEN bir kez çaðrýlýr.
        /// Örnek: Animasyonu durdur, timer'larý temizle.
        /// </summary>
        void Exit();
    }
}
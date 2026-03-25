using System;

namespace ToySiege.Core.FSM
{
    public class StateMachine
    {
        public IState CurrentState { get; private set; }
        public IState PreviousState { get; private set; }

        /// <summary>
        /// State deðiþtiðinde tetiklenir. Debug için çok faydalý.
        /// </summary>
        public event Action<IState, IState> OnStateChanged;

        /// <summary>
        /// Ýlk state'i belirler. Oyun baþladýðýnda bir kez çaðrýlýr.
        /// Genelde Idle state ile baþlatýlýr.
        /// </summary>
        public void Initialize(IState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
        }

        /// <summary>
        /// Yeni state'e geçiþ yapar.
        /// Ayný state'e tekrar geçmeyi engeller (gereksiz Enter/Exit önlenir).
        /// </summary>
        public void ChangeState(IState newState)
        {
            if (newState == null || newState == CurrentState) return;

            PreviousState = CurrentState;
            CurrentState.Exit();

            CurrentState = newState;
            CurrentState.Enter();

            OnStateChanged?.Invoke(PreviousState, CurrentState);
        }

        /// <summary>
        /// PlayerController.Update() içinden çaðrýlýr.
        /// </summary>
        public void Update()
        {
            CurrentState?.Execute();
        }

        /// <summary>
        /// PlayerController.FixedUpdate() içinden çaðrýlýr.
        /// </summary>
        public void FixedUpdate()
        {
            CurrentState?.FixedExecute();
        }
    }
}
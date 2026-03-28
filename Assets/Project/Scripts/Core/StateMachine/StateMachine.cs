

using System;

namespace ToySiege.Core.FSM
{
    public class StateMachine
    {
        public IState CurrentState { get; private set; }
        public IState PreviousState { get; private set; }

        /// <summary>
        /// State değiştiğinde tetiklenir. Debug için çok faydalı.
        /// </summary>
        public event Action<IState, IState> OnStateChanged;

        /// <summary>
        /// İlk state'i belirler. Oyun başladığında bir kez çağrılır.
        /// Genelde Idle state ile başlatılır.
        /// </summary>
        public void Initialize(IState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
        }

        /// <summary>
        /// Yeni state'e geçiş yapar.
        /// Aynı state'e tekrar geçmeyi engeller (gereksiz Enter/Exit önlenir).
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
        /// PlayerController.Update() içinden çağrılır.
        /// </summary>
        public void Update()
        {
            CurrentState?.Execute();
        }

        /// <summary>
        /// PlayerController.FixedUpdate() içinden çağrılır.
        /// </summary>
        public void FixedUpdate()
        {
            CurrentState?.FixedExecute();
        }
    }
}
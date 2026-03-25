using UnityEngine;

namespace ToySiege.Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        // ── Dışarıya açılan temiz property'ler ──
        // State'ler bunları okur

        /// <summary>
        /// WASD / Sol Stick → Vector2 (x: sağ-sol, y: ileri-geri)
        /// </summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>
        /// Space basıldı mı? (tek seferlik — okunduktan sonra sıfırlanır)
        /// </summary>
        public bool JumpPressed => ConsumeAction(ref _jumpPressed);

        /// <summary>
        /// Left Ctrl basıldı mı? (tek seferlik)
        /// </summary>
        public bool SlidePressed => ConsumeAction(ref _slidePressed);

        /// <summary>
        /// Sol Mouse tuşu basıldı mı? (tek seferlik)
        /// İleride saldırı sistemi için kullanılacak
        /// </summary>
        public bool AttackPressed => ConsumeAction(ref _attackPressed);

        /// <summary>
        /// Mouse X/Y hareketi (kamera kontrolü için)
        /// </summary>
        public Vector2 LookInput { get; private set; }

        // ── Dahili değişkenler ──
        private bool _jumpPressed;
        private bool _slidePressed;
        private bool _attackPressed;

        private void Update()
        {
            ReadMovement();
            ReadActions();
            ReadLook();
        }

        private void ReadMovement()
        {
            // WASD → Vector2
            float h = Input.GetAxisRaw("Horizontal"); // A/D
            float v = Input.GetAxisRaw("Vertical");   // W/S
            MoveInput = new Vector2(h, v).normalized;
        }

        private void ReadActions()
        {
            // GetKeyDown = sadece basıldığı frame'de true
            if (Input.GetKeyDown(KeyCode.Space))
                _jumpPressed = true;

            if (Input.GetKeyDown(KeyCode.LeftControl))
                _slidePressed = true;

            if (Input.GetMouseButtonDown(0)) // Sol tık
                _attackPressed = true;
        }

        private void ReadLook()
        {
            LookInput = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );
        }

        /// <summary>
        /// Bir action'ı okur ve hemen sıfırlar.
        /// Bu sayede bir tuş basışı sadece BİR KEZ tüketilir.
        /// Örnek: JumpPressed okundu → true döndü → artık false.
        /// </summary>
        private bool ConsumeAction(ref bool action)
        {
            if (!action) return false;
            action = false;
            return true;
        }
    }
}

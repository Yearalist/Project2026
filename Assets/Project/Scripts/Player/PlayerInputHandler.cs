

using UnityEngine;

namespace ToySiege.Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        // ── Hareket ──
        public Vector2 MoveInput { get; private set; }

        // ── Mouse (raw delta — hassasiyet PlayerConfig'de) ──
        public float MouseX { get; private set; }

        // ── Aksiyonlar ──
        public bool JumpPressed => ConsumeAction(ref _jumpPressed);
        public bool SlidePressed => ConsumeAction(ref _slidePressed);
        public bool AttackPressed => ConsumeAction(ref _attackPressed);

        // ── Sprint ──
        public bool SprintHeld { get; private set; }

        // ── Dahili ──
        private bool _jumpPressed;
        private bool _slidePressed;
        private bool _attackPressed;

        private void Update()
        {
            ReadMovement();
            ReadMouse();
            ReadActions();
        }

        private void ReadMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(h, v).normalized;
        }

        private void ReadMouse()
        {
            
            MouseX = Input.GetAxis("Mouse X");
        }

        private void ReadActions()
        {
            SprintHeld = Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKeyDown(KeyCode.Space))
                _jumpPressed = true;

            if (Input.GetKeyDown(KeyCode.LeftControl))
                _slidePressed = true;

            if (Input.GetMouseButtonDown(0))
                _attackPressed = true;
        }

        private bool ConsumeAction(ref bool action)
        {
            if (!action) return false;
            action = false;
            return true;
        }
    }
}
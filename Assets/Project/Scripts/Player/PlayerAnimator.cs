using UnityEngine;

namespace ToySiege.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        // PlayerAnimator.cs'e ekle
        public Animator GetAnimator() => _animator;
        private Animator _animator;
        private bool _hasAnimator;

        // ── Parametre Hash'leri ──
        private static readonly int MoveX = Animator.StringToHash("moveX");
        private static readonly int MoveY = Animator.StringToHash("moveY");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalSpd = Animator.StringToHash("VerticalSpeed");
        private static readonly int IsSprinting = Animator.StringToHash("IsSprinting");
        private static readonly int JumpTrigger = Animator.StringToHash("Jump");
        private static readonly int DoubleJumpTrigger = Animator.StringToHash("DoubleJump");
        private static readonly int SlideTrigger = Animator.StringToHash("Slide");
        private static readonly int LandTrigger = Animator.StringToHash("Land");

        // ── Smooth Damp ──
        private float _currentMoveX;
        private float _currentMoveY;
        private float _dampVelX;
        private float _dampVelY;
        private const float DampTime = 0.12f;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;

            if (!_hasAnimator)
                Debug.LogWarning("[PlayerAnimator] Animator bulunamadı!");
        }

        // ═══════════════════════════════════════
        //  LOCOMOTION — Blend Tree'yi süren metod
        // ═══════════════════════════════════════

        /// <summary>
        /// Her frame çağır. moveInput = PlayerInputHandler.MoveInput (normalized).
        /// isSprinting = true ise moveY değerini 1 yerine 2'ye çeker 
        /// → Blend Tree'de Slow Run / Strafe kliplerini seçtirir.
        /// </summary>
        public void UpdateLocomotion(Vector2 moveInput, bool isSprinting)
        {
            if (!_hasAnimator) return;

            // Sprint ise Y eksenini 2'ye scale et (Slow Run threshold'u)
            float targetX = moveInput.x;
            float targetY = moveInput.y;

            if (isSprinting && moveInput.sqrMagnitude > 0.01f)
            {
                targetX *= 2f;
                targetY *= 2f;
            }

            _currentMoveX = Mathf.SmoothDamp(_currentMoveX, targetX, ref _dampVelX, DampTime);
            _currentMoveY = Mathf.SmoothDamp(_currentMoveY, targetY, ref _dampVelY, DampTime);

            _animator.SetFloat(MoveX, _currentMoveX);
            _animator.SetFloat(MoveY, _currentMoveY);
        }

        /// <summary>
        /// Idle'a geçişte Blend Tree parametrelerini sıfırla.
        /// </summary>
        public void SetIdle()
        {
            UpdateLocomotion(Vector2.zero, false);
        }

        // ═══════════════════════════════════════
        //  BOOL / FLOAT PARAMETRELER
        // ═══════════════════════════════════════

        public void SetGrounded(bool grounded)
        {
            if (!_hasAnimator) return;
            _animator.SetBool(IsGrounded, grounded);
        }

        public void SetVerticalSpeed(float speed)
        {
            if (!_hasAnimator) return;
            _animator.SetFloat(VerticalSpd, speed);
        }

        public void SetSprinting(bool sprinting)
        {
            if (!_hasAnimator) return;
            _animator.SetBool(IsSprinting, sprinting);
        }
        // PlayerAnimator.cs'e ekle
        public void SetRootMotion(bool enabled)
        {
            if (_hasAnimator) _animator.applyRootMotion = enabled;
        }



        // ═══════════════════════════════════════
        //  TRIGGER'LAR
        // ═══════════════════════════════════════

        public void TriggerJump()
        {
            if (_hasAnimator) _animator.SetTrigger(JumpTrigger);
        }

        public void TriggerSlide()
        {
            Debug.Log($"<color=green>[Anim] TriggerSlide çağrıldı. " +
                      $"hasAnimator={_hasAnimator}, " +
                      $"currentState={(_hasAnimator ? _animator.GetCurrentAnimatorStateInfo(0).shortNameHash.ToString() : "N/A")}</color>");

            if (_hasAnimator) _animator.SetTrigger(SlideTrigger);
        }

        public void TriggerDoubleJump()
        {
            Debug.Log($"<color=magenta>[Anim] TriggerDoubleJump çağrıldı. " +
                      $"hasAnimator={_hasAnimator}, " +
                      $"currentState={(_hasAnimator ? _animator.GetCurrentAnimatorStateInfo(0).shortNameHash.ToString() : "N/A")}</color>");

            if (_hasAnimator) _animator.SetTrigger(DoubleJumpTrigger);
        }



        public void TriggerLanding()
        {
            if (_hasAnimator) _animator.SetTrigger(LandTrigger);
        }
    }
}
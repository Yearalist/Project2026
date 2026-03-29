using UnityEngine;

namespace ToySiege.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        private Animator _animator;
        private bool _hasAnimator;

        // ── Parametre Hash'leri (string lookup yerine int — performans) ──
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalSpd = Animator.StringToHash("VerticalSpeed");
        private static readonly int JumpTrigger = Animator.StringToHash("Jump");
        private static readonly int DoubleJumpTrigger = Animator.StringToHash("DoubleJump");
        private static readonly int SlideTrigger = Animator.StringToHash("Slide");
        private static readonly int LandTrigger = Animator.StringToHash("Land");

        // ── Smooth Damp (ani geçiş yerine yumuşak blend) ──
        private float _currentSpeed;
        private float _speedDampVelocity;
        private const float SpeedDampTime = 0.15f;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;

            if (!_hasAnimator)
                Debug.LogWarning("[PlayerAnimator] Animator bulunamadı — " +
                    "model child olarak eklendikten sonra otomatik çalışacak.");
        }

        // ═══════════════════════════════════════
        // STATE'LER TARAFINDAN ÇAĞRILAN METODLAR
        // ═══════════════════════════════════════

        /// <summary>
        /// Idle state'ine geçiş. Speed → 0
        /// </summary>
        public void SetIdle()
        {
            SetSpeed(0f);
        }

        /// <summary>
        /// Walk state. Speed → 1
        /// Blend Tree: 0-1 arası Idle→Walking geçişi
        /// </summary>
        public void SetWalking()
        {
            SetSpeed(1f);
        }

        /// <summary>
        /// Sprint state. Speed → 2
        /// Blend Tree: 1-2 arası Walking→Slow Run geçişi
        /// </summary>
        public void SetSprinting()
        {
            SetSpeed(2f);
        }

        /// <summary>
        /// Eski SetRunning uyumluluğu (Walk state'i hâlâ bunu çağırıyor)
        /// </summary>
        public void SetRunning(bool isRunning)
        {
            // Walk/Sprint state'leri artık SetWalking/SetSprinting kullanacak
            // ama eski state dosyaları güncellenmeden çalışsın diye:
            if (isRunning)
                SetSpeed(1f);
            else
                SetSpeed(0f);
        }

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

        public void TriggerJump()
        {
            Debug.Log("<color=cyan>[Anim] Jump</color>");
            if (_hasAnimator) _animator.SetTrigger(JumpTrigger);
        }

        public void TriggerDoubleJump()
        {
            Debug.Log("<color=cyan>[Anim] Double Jump</color>");
            if (_hasAnimator) _animator.SetTrigger(DoubleJumpTrigger);
        }

        public void TriggerSlide()
        {
            Debug.Log("<color=green>[Anim] Slide</color>");
            if (_hasAnimator) _animator.SetTrigger(SlideTrigger);
        }

        public void TriggerLanding()
        {
            Debug.Log("<color=yellow>[Anim] Landing</color>");
            if (_hasAnimator) _animator.SetTrigger(LandTrigger);
        }

        public void TriggerAttack()
        {
            Debug.Log("<color=red>[Anim] Attack</color>");
            // İleride attack animasyonu eklenince
        }

        // ═══════════════════════════════════════
        // YARDIMCI
        // ═══════════════════════════════════════

        /// <summary>
        /// Speed parametresini SmoothDamp ile yumuşak değiştirir.
        /// Idle(0) → Walk(1) → Sprint(2) arasında keskin geçiş yerine
        /// 0.15 saniyelik blend olur.
        /// </summary>
        private void SetSpeed(float target)
        {
            if (!_hasAnimator) return;

            _currentSpeed = Mathf.SmoothDamp(
                _currentSpeed, target,
                ref _speedDampVelocity, SpeedDampTime
            );
            _animator.SetFloat(Speed, _currentSpeed);
        }
    }
}
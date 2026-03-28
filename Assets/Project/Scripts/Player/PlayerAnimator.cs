
using UnityEngine;

namespace ToySiege.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        private Animator _animator;
        private bool _hasAnimator;

        
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int JumpTrigger = Animator.StringToHash("Jump");
        private static readonly int DoubleJumpTrigger = Animator.StringToHash("DoubleJump");
        private static readonly int SlideTrigger = Animator.StringToHash("Slide");
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;

            if (!_hasAnimator)
                Debug.Log("<color=yellow>[PlayerAnimator] Animator bulunamadı — " +
                          "debug modda çalışılıyor. Model eklendiğinde otomatik çalışacak.</color>");
        }

        public void SetRunning(bool isRunning)
        {
            if (_hasAnimator) _animator.SetBool(IsRunning, isRunning);
        }

        public void SetGrounded(bool grounded)
        {
            if (_hasAnimator) _animator.SetBool(IsGrounded, grounded);
        }

        public void SetVerticalSpeed(float speed)
        {
            if (_hasAnimator) _animator.SetFloat(VerticalSpeed, speed);
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

        public void TriggerAttack()
        {
            Debug.Log("<color=red>[Anim] Attack</color>");
            if (_hasAnimator) _animator.SetTrigger(AttackTrigger);
        }

        public void SetIdle()
        {
            SetRunning(false);
        }
    }
}
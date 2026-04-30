using UnityEngine;

namespace ToySiege.Enemy
{
    public class EnemyAnimator : MonoBehaviour
    {
        private Animator _animator;
        private bool _hasAnimator;

        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        private static readonly int HitTrigger = Animator.StringToHash("Hit");
        private static readonly int DieTrigger = Animator.StringToHash("Die");

        private float _currentSpeed;
        private float _dampVel;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;
        }

        public void UpdateSpeed(float speed)
        {
            if (!_hasAnimator) return;
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, speed, ref _dampVel, 0.1f);
            _animator.SetFloat(Speed, _currentSpeed);
        }

        public void TriggerAttack()
        {
            if (_hasAnimator) _animator.SetTrigger(AttackTrigger);
        }

        public void TriggerHit()
        {
            if (_hasAnimator) _animator.SetTrigger(HitTrigger);
        }

        public void TriggerDie()
        {
            if (_hasAnimator) _animator.SetTrigger(DieTrigger);
        }
    }
}
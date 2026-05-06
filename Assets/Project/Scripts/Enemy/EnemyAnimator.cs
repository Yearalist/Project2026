using UnityEngine;

namespace ToySiege.Enemy
{
    public class EnemyAnimator : MonoBehaviour
    {
        private Animator _animator;
        private bool _hasAnimator;

        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int InCombat = Animator.StringToHash("InCombat");
        private static readonly int AttackIndex = Animator.StringToHash("AttackIndex");
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        private static readonly int HitTrigger = Animator.StringToHash("Hit");
        private static readonly int DieTrigger = Animator.StringToHash("Die");

        private float _currentSpeed;
        private float _dampVel;
        private const int TotalAttacks = 5;
        private int _lastAttackIndex = -1;

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

        public void SetCombatMode(bool inCombat)
        {
            if (!_hasAnimator) return;
            _animator.SetBool(InCombat, inCombat);
        }

        public void TriggerAttack()
        {
            if (!_hasAnimator) return;

            int index = Random.Range(0, TotalAttacks);
            while (index == _lastAttackIndex && TotalAttacks > 1)
                index = Random.Range(0, TotalAttacks);

            _lastAttackIndex = index;
            _animator.SetInteger(AttackIndex, index);
            _animator.SetTrigger(AttackTrigger);

            string[] names = { "Punch1", "Punch2", "Punch3", "Kick1", "Kick2" };
            Debug.Log($"<color=red>[Enemy Anim] Saldýrý: {names[index]}</color>");
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
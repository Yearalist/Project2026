using System;
using UnityEngine;

namespace ToySiege.Player.Health
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _invincibleDuration = 0.5f;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        private float _invincibleTimer;
        private bool IsInvincible => _invincibleTimer > 0f;

        // Event'ler — UI ve efektler bunlarý dinleyecek
        public event Action<float, float> OnHealthChanged;      // (current, max)
        public event Action<float> OnDamageTaken;                // (damage miktarý)
        public event Action OnDeath;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
        }

        private void Update()
        {
            if (_invincibleTimer > 0f)
                _invincibleTimer -= Time.deltaTime;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || IsInvincible) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            _invincibleTimer = _invincibleDuration;

            Debug.Log($"<color=red>[HP] Hasar: {damage} | Kalan: {CurrentHealth}/{_maxHealth}</color>");

            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);

            if (IsDead)
            {
                Debug.Log("<color=red>[HP] ÖLDÜ!</color>");
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            CurrentHealth = Mathf.Min(_maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = _maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        }
    }
}
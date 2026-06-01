using UnityEngine;

namespace ToySiege.Enemy
{
    /// <summary>
    /// Düşman mermisi. EnemyProjectileAttack tarafından spawn edilir.
    /// İleri doğru hareket eder, oyuncuya çarparsa hasar verir,
    /// herhangi bir şeye çarparsa yok olur.
    ///
    /// KURULUM:
    ///   1) Küçük bir Sphere/Capsule oluştur → Rigidbody (Use Gravity = false) + Collider (Is Trigger = true)
    ///   2) Bu script'i ekle
    ///   3) Trail Renderer ekle (opsiyonel — görsel iz)
    ///   4) Prefab yap
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyProjectile : MonoBehaviour
    {
        [Header("VFX")]
        [SerializeField] private GameObject _impactEffectPrefab;

        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private float _lifetime;
        private Rigidbody _rb;
        private bool _hasHit;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.isKinematic = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        /// <summary>
        /// Mermi parametrelerini ayarla. Spawn sonrası çağrılır.
        /// </summary>
        public void Initialize(Vector3 direction, float speed, float damage, float lifetime)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _lifetime = lifetime;

            _rb.linearVelocity = _direction * _speed;

            Destroy(gameObject, _lifetime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit) return;

            // Kendi düşman arkadaşlarına çarpmasın
            if (other.CompareTag("Enemy")) return;

            _hasHit = true;

            // Oyuncuya hasar
            if (other.CompareTag("Player"))
            {
                var playerHealth = other.GetComponent<ToySiege.Player.Health.PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(_damage);

                Debug.Log($"<color=red>[EnemyProjectile] Oyuncuya isabet! Hasar: {_damage}</color>");
            }

            // Impact efekti
            if (_impactEffectPrefab != null)
            {
                Instantiate(_impactEffectPrefab, transform.position,
                    Quaternion.LookRotation(-_direction));
            }

            Destroy(gameObject);
        }
    }
}

using UnityEngine;

namespace ToySiege.Enemy
{
    /// <summary>
    /// Uzak mesafe saldırı component'i.
    ///
    /// Mevcut EnemyController + FSM altyapısıyla çalışır.
    /// EnemyAttackState'in Enter() sırasında bu component varsa
    /// yakın dövüş yerine mermi (projectile) fırlatır.
    ///
    /// Bu component'i ayrı tutarak mevcut yakın dövüş düşmanlarını
    /// değiştirmeden yeni bir düşman tipi oluşturabilirsin:
    ///   - Aynı EnemyController + FSM
    ///   - Farklı EnemyConfig (daha yüksek AttackRange, daha düşük AttackDamage)
    ///   - Bu component eklendi
    ///
    /// KURULUM:
    ///   1) Düşman prefab'ına EnemyProjectileAttack ekle
    ///   2) _projectilePrefab → mermi prefab'ı ata (EnemyProjectile component'li)
    ///   3) _muzzle → ateş noktası (silah ucu Transform)
    ///   4) EnemyConfig'te AttackRange'ı 8-12 yap (uzak mesafe)
    /// </summary>
    public class EnemyProjectileAttack : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _muzzle;

        [Header("Mermi Ayarları")]
        [SerializeField] private float _projectileSpeed = 15f;
        [SerializeField] private float _damage = 8f;
        [SerializeField] private float _lifetime = 5f;

        [Header("VFX")]
        [SerializeField] private ParticleSystem _muzzleFlash;

        private EnemyController _controller;

        private void Awake()
        {
            _controller = GetComponent<EnemyController>();

            if (_muzzle == null)
            {
                // Fallback: objenin kendi pozisyonu + ileri yönde biraz offset
                Debug.LogWarning("[EnemyProjectile] Muzzle atanmamış, varsayılan kullanılıyor.");
            }
        }

        /// <summary>
        /// Mermi fırlat. EnemyAttackState.Enter() tarafından çağrılır.
        /// </summary>
        public void FireProjectile()
        {
            if (_projectilePrefab == null) return;

            Vector3 spawnPos = _muzzle != null
                ? _muzzle.position
                : transform.position + transform.forward * 0.5f + Vector3.up;

            Vector3 direction;
            if (_controller != null && _controller.Detection.HasTarget)
            {
                // Hedefe doğru nişan al (biraz yukarı — göğüs bölgesi)
                Vector3 targetPos = _controller.Detection.Target.position + Vector3.up * 1f;
                direction = (targetPos - spawnPos).normalized;
            }
            else
            {
                direction = transform.forward;
            }

            Quaternion rotation = Quaternion.LookRotation(direction);
            GameObject projObj = Instantiate(_projectilePrefab, spawnPos, rotation);

            var projectile = projObj.GetComponent<EnemyProjectile>();
            if (projectile != null)
                projectile.Initialize(direction, _projectileSpeed, _damage, _lifetime);

            if (_muzzleFlash != null)
                _muzzleFlash.Play();
        }
    }
}

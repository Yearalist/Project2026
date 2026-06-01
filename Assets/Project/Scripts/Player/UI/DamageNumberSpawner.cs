using UnityEngine;
using ToySiege.Core;
using ToySiege.Player.Combat;

namespace ToySiege.UI
{
    /// <summary>
    /// Weapon.OnHit event'ini dinler, isabet noktasında FloatingDamageText spawn eder.
    /// Object Pool kullanarak GC allocation'ı önler.
    ///
    /// KURULUM:
    ///   1) Sahneye boş GameObject ekle, DamageNumberSpawner ata
    ///   2) FloatingDamageText prefab'ını _damageTextPrefab'a ata
    ///   3) Weapon referansını ata
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private Weapon _weapon;
        [SerializeField] private GameObject _damageTextPrefab;

        [Header("Pool")]
        [SerializeField] private int _poolSize = 15;

        [Header("Offset")]
        [Tooltip("Hasar sayısının isabet noktasından yukarı offset'i")]
        [SerializeField] private float _spawnOffset = 0.5f;

        private ObjectPool _pool;

        private void Awake()
        {
            if (_damageTextPrefab != null)
                _pool = new ObjectPool(_damageTextPrefab, transform, _poolSize);
        }

        private void OnEnable()
        {
            if (_weapon != null)
                _weapon.OnHit += SpawnDamageNumber;
        }

        private void OnDisable()
        {
            if (_weapon != null)
                _weapon.OnHit -= SpawnDamageNumber;
        }

        private void SpawnDamageNumber(float damage, Vector3 worldPos)
        {
            if (_pool == null) return;

            Vector3 spawnPos = worldPos + Vector3.up * _spawnOffset;
            GameObject obj = _pool.Get(spawnPos, Quaternion.identity);

            var floatingText = obj.GetComponent<FloatingDamageText>();
            if (floatingText != null)
                floatingText.Play(damage, spawnPos);
        }
    }
}

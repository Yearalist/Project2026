using System;
using UnityEngine;
using ToySiege.Combat;
using ToySiege.Core;

namespace ToySiege.Player.Combat
{
    public class Weapon : MonoBehaviour
    {
        // ── Event'ler — UI bu event'leri dinleyecek ──
        /// <summary> (currentAmmo, maxAmmo) </summary>
        public event Action<int, int> OnAmmoChanged;
        /// <summary> Ateş edildiğinde tetiklenir </summary>
        public event Action OnFired;
        /// <summary> IDamageable'a isabet ettiğinde (damage, worldPos) </summary>
        public event Action<float, Vector3> OnHit;

        [Header("Ateş Ayarları")]
        [SerializeField] private float _damage = 25f;
        [SerializeField] private float _range = 100f;
        [SerializeField] private float _fireRate = 0.15f;
        [SerializeField] private bool _isAutomatic = true;
        [SerializeField] private LayerMask _hitMask = ~0;     // ~0 = TÜM LAYER'lar

        [Header("Mermi/Hasar")]
        [SerializeField] private int _maxAmmo = 30;
        [SerializeField] private float _impactForce = 50f;

        [Header("Referanslar")]
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Camera _aimCamera;

        [Header("VFX")]
        [SerializeField] private ParticleSystem _muzzleFlash;
        [SerializeField] private GameObject _impactEffectPrefab;
        [SerializeField] private TrailRenderer _bulletTrailPrefab;

        [Header("Pool Ayarları")]
        [SerializeField] private int _impactPoolSize = 10;
        [SerializeField] private int _trailPoolSize = 10;

        [Header("Debug")]
        [SerializeField] private bool _showDebugRay = true;

        public int CurrentAmmo { get; private set; }
        public int MaxAmmo => _maxAmmo;
        public bool IsAutomatic => _isAutomatic;
        public bool HasAmmo => CurrentAmmo > 0;
        public float FireRate => _fireRate;

        private float _nextFireTime;
        private ObjectPool _impactPool;
        private ObjectPool _trailPool;

        private void Awake()
        {
            CurrentAmmo = _maxAmmo;
            if (_aimCamera == null) _aimCamera = Camera.main;

            if (_aimCamera == null)
                Debug.LogError("[Weapon] Aim Camera bulunamadı! Camera.main null.");
            if (_muzzle == null)
                Debug.LogError("[Weapon] Muzzle Transform atanmamış!");

            // Object Pool'ları oluştur
            if (_impactEffectPrefab != null)
                _impactPool = new ObjectPool(_impactEffectPrefab, transform, _impactPoolSize);

            if (_bulletTrailPrefab != null)
                _trailPool = new ObjectPool(_bulletTrailPrefab.gameObject, transform, _trailPoolSize);
        }

        public bool CanFire() => Time.time >= _nextFireTime && HasAmmo;

        public void Fire()
        {
            if (!CanFire()) return;

            _nextFireTime = Time.time + _fireRate;
            CurrentAmmo--;

            OnAmmoChanged?.Invoke(CurrentAmmo, _maxAmmo);
            OnFired?.Invoke();

            if (_muzzleFlash != null) _muzzleFlash.Play();

            // Ekran ortasından ray
            Ray ray = _aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            // DEBUG: Sahnede ray'i göster
            if (_showDebugRay)
                Debug.DrawRay(ray.origin, ray.direction * _range, Color.red, 1f);

            Vector3 hitPoint = ray.origin + ray.direction * _range;
            bool didHit = Physics.Raycast(ray, out RaycastHit hit, _range, _hitMask);

            if (didHit)
            {
                hitPoint = hit.point;

                // DEBUG: Neye çarptığını yazdır
                Debug.Log($"<color=yellow>[Weapon] Çarptı: {hit.collider.name} | Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)} | Tag: {hit.collider.tag}</color>");

                // IDamageable arama — collider'ın kendisinde ve parent'larında
                var damageable = hit.collider.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    Debug.Log($"<color=green>[Weapon] IDamageable bulundu! Hasar: {_damage}</color>");
                    damageable.TakeDamage(_damage, hit.point, ray.direction);
                    OnHit?.Invoke(_damage, hit.point);
                }
                else
                {
                    Debug.Log($"<color=orange>[Weapon] IDamageable YOK! Collider: {hit.collider.name}, Parent: {(hit.collider.transform.parent != null ? hit.collider.transform.parent.name : "yok")}</color>");
                }

                if (hit.rigidbody != null)
                    hit.rigidbody.AddForceAtPosition(ray.direction * _impactForce, hit.point);

                if (_impactPool != null)
                {
                    GameObject impact = _impactPool.Get(
                        hit.point, Quaternion.LookRotation(hit.normal));
                    StartCoroutine(ReturnToPool(_impactPool, impact, 2f));
                }
            }
            else
            {
                Debug.Log("<color=grey>[Weapon] Ateş — bir şeye çarpmadı</color>");
            }

            if (_muzzle != null)
                SpawnTrail(_muzzle.position, hitPoint);

            Debug.Log($"<color=yellow>[Weapon] Ateş! Mermi: {CurrentAmmo}/{_maxAmmo}</color>");
        }

        private void SpawnTrail(Vector3 from, Vector3 to)
        {
            if (_trailPool == null) return;

            GameObject trailObj = _trailPool.Get(from, Quaternion.identity);
            TrailRenderer trail = trailObj.GetComponent<TrailRenderer>();
            if (trail != null) trail.Clear();
            StartCoroutine(MoveTrail(trailObj, trail, to));
        }

        private System.Collections.IEnumerator MoveTrail(GameObject trailObj, TrailRenderer trail, Vector3 target)
        {
            Vector3 start = trailObj.transform.position;
            float distance = Vector3.Distance(start, target);
            float speed = 120f;
            float t = 0f;

            while (t < 1f)
            {
                trailObj.transform.position = Vector3.Lerp(start, target, t);
                t += Time.deltaTime * speed / distance;
                yield return null;
            }

            trailObj.transform.position = target;

            // Trail'in sönmesini bekle, sonra havuza geri koy
            float waitTime = trail != null ? trail.time : 0.5f;
            yield return new WaitForSeconds(waitTime);
            _trailPool.Return(trailObj);
        }

        private System.Collections.IEnumerator ReturnToPool(ObjectPool pool, GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            pool.Return(obj);
        }

        public void Reload()
        {
            CurrentAmmo = _maxAmmo;
            Debug.Log("<color=cyan>[Weapon] Reload!</color>");
        }
    }
}
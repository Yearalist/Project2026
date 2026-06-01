using UnityEngine;
using UnityEngine.AI;

namespace ToySiege.Enemy
{
    /// <summary>
    /// Düşman ragdoll sistemi.
    ///
    /// Ölüm anında Animator'ı kapatır, ragdoll collider/rigidbody'leri aktif eder
    /// ve son vuruş yönünde bir kuvvet uygular.
    /// Belirli süre sonra objeyi yok eder veya havuza geri döndürür.
    ///
    /// KURULUM:
    ///   1) Düşman modeline Unity'de ragdoll ekle (Create Ragdoll wizard)
    ///   2) Bu script'i düşman root objesi'ne ekle
    ///   3) _animator referansını ata (model üzerindeki Animator)
    ///   4) Başlangıçta ragdoll rigidbody'ler isKinematic, collider'lar disabled olmalı
    ///      (script bunu Awake'te otomatik yapar)
    /// </summary>
    public class EnemyRagdoll : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private Animator _animator;

        [Header("Ragdoll Ayarları")]
        [Tooltip("Vuruş kuvveti çarpanı")]
        [SerializeField] private float _hitForceMultiplier = 10f;
        [Tooltip("Yukarı itme kuvveti — düşman biraz havaya kalksın")]
        [SerializeField] private float _upwardForce = 3f;
        [Tooltip("Ragdoll aktif olduktan kaç saniye sonra yok edilsin")]
        [SerializeField] private float _destroyDelay = 4f;
        [Tooltip("Yok edilmeden önce fade-out (alpha azaltma) süresi")]
        [SerializeField] private float _fadeOutDuration = 1.5f;

        private Rigidbody[] _ragdollBodies;
        private Collider[] _ragdollColliders;
        private NavMeshAgent _agent;
        private Renderer[] _renderers;
        private bool _isActive;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();

            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            // Model altındaki tüm ragdoll parçalarını bul
            _ragdollBodies = _animator != null
                ? _animator.GetComponentsInChildren<Rigidbody>()
                : GetComponentsInChildren<Rigidbody>();

            _ragdollColliders = _animator != null
                ? _animator.GetComponentsInChildren<Collider>()
                : GetComponentsInChildren<Collider>();

            _renderers = GetComponentsInChildren<Renderer>();

            // Başlangıçta ragdoll kapalı
            SetRagdollActive(false);
        }

        /// <summary>
        /// Ragdoll'ü aktif eder ve vuruş yönünde kuvvet uygular.
        /// EnemyController.TakeDamage tarafından çağrılır.
        /// </summary>
        public void ActivateRagdoll(Vector3 hitPoint, Vector3 hitDirection)
        {
            if (_isActive) return;
            _isActive = true;

            // Animator'ı kapat → fizik devralacak
            if (_animator != null)
                _animator.enabled = false;

            // NavMeshAgent'ı kapat
            if (_agent != null)
                _agent.enabled = false;

            // EnemyController ve diğer behaviour'ları kapat
            var controller = GetComponent<EnemyController>();
            if (controller != null)
                controller.enabled = false;

            var detection = GetComponent<EnemyDetection>();
            if (detection != null)
                detection.enabled = false;

            // Ragdoll aç
            SetRagdollActive(true);

            // Vuruş kuvveti uygula — isabet noktasına en yakın rigidbody'ye
            ApplyHitForce(hitPoint, hitDirection);

            // Yok etme zamanlayıcısı
            StartCoroutine(DestroyAfterDelay());
        }

        /// <summary>
        /// Ragdoll olmadan basit ölüm — animasyon oynasın, sonra yok et.
        /// Ragdoll setup'ı olmayan düşmanlar için fallback.
        /// </summary>
        public void ActivateSimpleDeath()
        {
            if (_isActive) return;
            _isActive = true;

            if (_agent != null)
                _agent.enabled = false;

            StartCoroutine(DestroyAfterDelay());
        }

        private void SetRagdollActive(bool active)
        {
            foreach (var rb in _ragdollBodies)
            {
                // Root objesi'nin kendi Rigidbody'sini atla
                if (rb.gameObject == gameObject) continue;

                rb.isKinematic = !active;
                rb.useGravity = active;

                if (active)
                {
                    rb.linearDamping = 0.5f;
                    rb.angularDamping = 0.5f;
                }
            }

            foreach (var col in _ragdollColliders)
            {
                if (col.gameObject == gameObject) continue;

                // NavMeshAgent'ın kendi collider'ını atla
                if (col is CharacterController) continue;

                col.enabled = active;
            }
        }

        private void ApplyHitForce(Vector3 hitPoint, Vector3 hitDirection)
        {
            if (_ragdollBodies.Length == 0) return;

            // İsabet noktasına en yakın rigidbody'yi bul
            Rigidbody closestRb = null;
            float closestDist = float.MaxValue;

            foreach (var rb in _ragdollBodies)
            {
                if (rb.gameObject == gameObject) continue;

                float dist = Vector3.Distance(rb.position, hitPoint);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestRb = rb;
                }
            }

            if (closestRb != null)
            {
                Vector3 force = hitDirection.normalized * _hitForceMultiplier
                              + Vector3.up * _upwardForce;
                closestRb.AddForce(force, ForceMode.VelocityChange);
            }
        }

        private System.Collections.IEnumerator DestroyAfterDelay()
        {
            float waitBeforeFade = _destroyDelay - _fadeOutDuration;
            if (waitBeforeFade > 0f)
                yield return new WaitForSeconds(waitBeforeFade);

            // Fade out — renderer'ların material alpha'sını düşür
            float elapsed = 0f;
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

            while (elapsed < _fadeOutDuration)
            {
                float alpha = 1f - (elapsed / _fadeOutDuration);
                foreach (var rend in _renderers)
                {
                    if (rend == null) continue;
                    rend.GetPropertyBlock(propBlock);
                    propBlock.SetColor("_BaseColor",
                        new Color(1f, 1f, 1f, alpha));
                    rend.SetPropertyBlock(propBlock);
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}

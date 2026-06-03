using System;
using UnityEngine;

namespace ToySiege.Core
{
    /// <summary>
    /// Kilitli kapı — tüm anahtarlar toplandığında açılır.
    /// </summary>
    public class Door : MonoBehaviour
    {
        public event Action OnDoorUnlocked;
        public event Action OnPlayerEntered;

        [Header("Durum")]
        [SerializeField] private bool _isLocked = true;

        [Header("Görsel")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Material _lockedMaterial;
        [SerializeField] private Material _unlockedMaterial;
        [Tooltip("Materyali değiştirilecek renderer — boş bırakırsan otomatik bulur")]
        [SerializeField] private Renderer _doorRenderer;

        [Header("Ses")]
        [SerializeField] private AudioClip _unlockSFX;
        [SerializeField] private AudioClip _enterSFX;
        [Tooltip("Kilitli kapıya dokunulduğunda çalan ses")]
        [SerializeField] private AudioClip _lockedSFX;

        [Header("Geçiş")]
        [SerializeField] private float _transitionDelay = 1f;

        private static readonly int OpenTrigger = Animator.StringToHash("Open");
        private bool _hasTriggeredTransition;
        private MaterialPropertyBlock _propBlock;

        public bool IsLocked => _isLocked;

        private void Start()
        {
            _propBlock = new MaterialPropertyBlock();

            // Renderer otomatik bul
            if (_doorRenderer == null)
                _doorRenderer = GetComponentInChildren<Renderer>();

            // Başlangıçta kilitli görsel
            ApplyMaterial(_lockedMaterial);

            // Event dinleme
            if (LevelManager.Instance != null)
                LevelManager.Instance.OnAllKeysCollected += Unlock;
        }

        public void Unlock()
        {
            if (!_isLocked) return;

            _isLocked = false;
            Debug.Log("<color=green>[Door] Kapı açıldı!</color>");

            // Materyal değiştir
            ApplyMaterial(_unlockedMaterial);

            // Animasyon
            if (_animator != null)
                _animator.SetTrigger(OpenTrigger);

            // Açılma sesi
            PlaySFX(_unlockSFX);

            OnDoorUnlocked?.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            // Kilitli — kilitli ses çal ve geri dön
            if (_isLocked)
            {
                PlaySFX(_lockedSFX);
                Debug.Log("<color=red>[Door] Kapı kilitli! Tüm anahtarları topla.</color>");
                return;
            }

            if (_hasTriggeredTransition) return;

            _hasTriggeredTransition = true;
            Debug.Log("<color=green>[Door] Seviye tamamlandı!</color>");

            OnPlayerEntered?.Invoke();
            PlaySFX(_enterSFX);

            Invoke(nameof(TriggerNextLevel), _transitionDelay);
        }

        private void TriggerNextLevel()
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.NextLevel();
        }

        /// <summary>
        /// Kapının materyalini değiştirir.
        /// sharedMaterial kullanarak material instance leak'ini önler.
        /// </summary>
        private void ApplyMaterial(Material mat)
        {
            if (_doorRenderer == null || mat == null) return;

            // Her material slot'a uygula
            Material[] mats = _doorRenderer.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            _doorRenderer.sharedMaterials = mats;
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(clip, transform.position);
            else
                AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
                LevelManager.Instance.OnAllKeysCollected -= Unlock;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ToySiege.Core
{
    /// <summary>
    /// Seviye yöneticisi — Singleton.
    ///
    /// Anahtar toplama, kapı açma ve sahne geçiş mantığını yönetir.
    /// Event-driven: UI ve diğer sistemler event'leri dinleyerek tepki verir.
    ///
    /// KURULUM:
    ///   1) Sahneye boş GameObject: LevelManager → bu script'i ekle
    ///   2) Inspector'da Required Keys = 4, Next Scene Name = sonraki sahne adı
    ///   3) Door referansını ata (opsiyonel — event ile de çalışır)
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        // ── Event'ler ──
        /// <summary> Anahtar toplandığında (mevcutAnahtar, gerekenAnahtar) </summary>
        public event Action<int, int> OnKeyCollected;
        /// <summary> Tüm anahtarlar toplandığında </summary>
        public event Action OnAllKeysCollected;
        /// <summary> Seviye geçişi başladığında </summary>
        public event Action OnLevelCompleted;

        [Header("Anahtar Ayarları")]
        [SerializeField] private int _requiredKeys = 4;

        [Header("Sahne Geçişi")]
        [Tooltip("Sonraki sahnenin adı. Boş bırakılırsa build index + 1 kullanılır.")]
        [SerializeField] private string _nextSceneName = "";

        [Header("Referanslar (Opsiyonel)")]
        [Tooltip("Kapı referansı. Boş bırakılırsa OnAllKeysCollected event'i ile çalışır.")]
        [SerializeField] private Door _door;

        public int CurrentKeys { get; private set; }
        public int RequiredKeys => _requiredKeys;
        public bool AllKeysCollected => CurrentKeys >= _requiredKeys;

        private void Awake()
        {
            // Singleton — sahne bazlı (DontDestroyOnLoad yok, her seviye kendi LevelManager'ı)
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Anahtar toplandığında çağrılır. KeyItem tarafından tetiklenir.
        /// </summary>
        public void AddKey()
        {
            CurrentKeys++;
            OnKeyCollected?.Invoke(CurrentKeys, _requiredKeys);

            Debug.Log($"<color=yellow>[Level] Anahtar toplandı! {CurrentKeys}/{_requiredKeys}</color>");

            if (AllKeysCollected)
            {
                Debug.Log("<color=green>[Level] Tüm anahtarlar toplandı! Kapı açılıyor!</color>");
                OnAllKeysCollected?.Invoke();

                // Doğrudan kapı referansı varsa tetikle
                if (_door != null)
                    _door.Unlock();
            }
        }

        /// <summary>
        /// Sonraki seviyeye geçiş. Door tarafından çağrılır.
        /// </summary>
        public void NextLevel()
        {
            OnLevelCompleted?.Invoke();
            Debug.Log("<color=green>[Level] Sonraki seviyeye geçiliyor!</color>");

            if (!string.IsNullOrEmpty(_nextSceneName))
            {
                SceneManager.LoadScene(_nextSceneName);
            }
            else
            {
                // Build index + 1 ile sonraki sahneye geç
                int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

                // Son sahnedeyse ilk sahneye dön (loop)
                if (nextIndex >= SceneManager.sceneCountInBuildSettings)
                    nextIndex = 0;

                SceneManager.LoadScene(nextIndex);
            }
        }

        /// <summary>
        /// Mevcut seviyeyi yeniden başlat.
        /// </summary>
        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}

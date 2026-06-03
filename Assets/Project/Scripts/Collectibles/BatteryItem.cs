using UnityEngine;

namespace ToySiege.Collectibles
{
    /// <summary>
    /// Can yenileme paketi (Pil / Battery).
    ///
    /// Oyuncu temas ettiğinde PlayerHealth.Heal() çağırarak
    /// maks canın %10'u kadar iyileştirme yapar. Can zaten
    /// maksimumsa toplanmaz (israf olmasın).
    ///
    /// KURULUM:
    ///   1) Pil modeline Collider ekle → Is Trigger = ✓
    ///   2) Bu script'i ekle
    ///   3) (Opsiyonel) VFX/SFX referansları ata
    ///   4) Sahneye istediğin kadar koy
    ///
    /// NOT (2D): OnTriggerEnter → OnTriggerEnter2D, Collider → Collider2D
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BatteryItem : MonoBehaviour
    {
        [Header("İyileştirme")]
        [Tooltip("Maks canın yüzde kaçı kadar iyileştirme yapılacak (0.10 = %10)")]
        [SerializeField, Range(0.01f, 1f)] private float _healPercent = 0.10f;
        [Tooltip("Can zaten doluysa toplanmasın")]
        [SerializeField] private bool _skipIfFullHealth = true;

        [Header("Görsel / Ses")]
        [SerializeField] private GameObject _collectVFX;
        [SerializeField] private AudioClip _collectSFX;

        [Header("Animasyon")]
        [SerializeField] private float _rotateSpeed = 60f;
        [SerializeField] private float _bobAmplitude = 0.15f;
        [SerializeField] private float _bobSpeed = 1.5f;

        private Vector3 _startPos;

        private void Start()
        {
            _startPos = transform.position;

            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning($"[BatteryItem] {gameObject.name} collider'ı Trigger'a çevrildi.");
            }
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime, Space.World);

            Vector3 pos = _startPos;
            pos.y += Mathf.Sin(Time.time * _bobSpeed) * _bobAmplitude;
            transform.position = pos;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            // PlayerHealth bul
            var health = other.GetComponent<Player.Health.PlayerHealth>();
            if (health == null)
            {
                health = other.GetComponentInParent<Player.Health.PlayerHealth>();
            }

            if (health == null)
            {
                Debug.LogWarning("[BatteryItem] PlayerHealth bulunamadı!");
                return;
            }

            // Can doluysa toplama
            if (_skipIfFullHealth && health.CurrentHealth >= health.MaxHealth)
            {
                Debug.Log("<color=grey>[Battery] Can zaten dolu, pil toplanmadı.</color>");
                return;
            }

            // Maks canın belirlenen yüzdesi kadar iyileştir
            float healAmount = health.MaxHealth * _healPercent;
            health.Heal(healAmount);

            Debug.Log($"<color=green>[Battery] +{healAmount:F0} HP iyileştirme! ({_healPercent * 100f}%)</color>");

            // VFX — objenin pozisyonunda, küçük scale
            if (_collectVFX != null)
            {
                GameObject vfx = Instantiate(_collectVFX, transform.position, Quaternion.identity);
                vfx.transform.localScale = Vector3.one * 0.5f;

                if (vfx.GetComponent<Core.AutoDestroyVFX>() == null)
                    Destroy(vfx, 2f);
            }

            // SFX
            if (_collectSFX != null)
            {
                if (Core.AudioManager.Instance != null)
                    Core.AudioManager.Instance.PlaySFX(_collectSFX, transform.position);
                else
                    AudioSource.PlayClipAtPoint(_collectSFX, transform.position);
            }

            Destroy(gameObject);
        }

        // ── 2D KULLANIM İÇİN ──
        // private void OnTriggerEnter2D(Collider2D other)
        // {
        //     if (!other.CompareTag("Player")) return;
        //     var health = other.GetComponent<Player.Health.PlayerHealth>();
        //     // ... aynı içerik ...
        // }
    }
}

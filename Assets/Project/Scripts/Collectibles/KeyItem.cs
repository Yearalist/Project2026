using UnityEngine;

namespace ToySiege.Collectibles
{
    /// <summary>
    /// Toplanabilir anahtar.
    ///
    /// Oyuncu temas ettiğinde LevelManager'daki anahtar sayısını artırır
    /// ve kendini yok eder. Opsiyonel olarak toplama VFX/SFX çalar.
    ///
    /// KURULUM:
    ///   1) Anahtar modeline Collider ekle → Is Trigger = ✓
    ///   2) Bu script'i ekle
    ///   3) (Opsiyonel) _collectVFX ve _collectSFX referansları ata
    ///   4) Sahneye 4 adet koy
    ///
    /// NOT (2D): OnTriggerEnter → OnTriggerEnter2D, Collider → Collider2D
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class KeyItem : MonoBehaviour
    {
        [Header("Görsel / Ses")]
        [SerializeField] private GameObject _collectVFX;
        [SerializeField] private AudioClip _collectSFX;

        [Header("Animasyon")]
        [Tooltip("Anahtar havada dönme hızı (derece/saniye)")]
        [SerializeField] private float _rotateSpeed = 90f;
        [Tooltip("Yukarı-aşağı sallanma genliği")]
        [SerializeField] private float _bobAmplitude = 0.2f;
        [SerializeField] private float _bobSpeed = 2f;

        private Vector3 _startPos;

        private void Start()
        {
            _startPos = transform.position;

            // Collider'ın Trigger olduğundan emin ol
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning($"[KeyItem] {gameObject.name} collider'ı Trigger'a çevrildi.");
            }
        }

        private void Update()
        {
            // Dönme animasyonu
            transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime, Space.World);

            // Yukarı-aşağı sallanma
            Vector3 pos = _startPos;
            pos.y += Mathf.Sin(Time.time * _bobSpeed) * _bobAmplitude;
            transform.position = pos;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            // LevelManager'a bildir
            if (Core.LevelManager.Instance != null)
            {
                Core.LevelManager.Instance.AddKey();
            }
            else
            {
                Debug.LogError("[KeyItem] LevelManager bulunamadı! Sahneye LevelManager ekle.");
            }

            // VFX — objenin pozisyonunda, küçük scale
            if (_collectVFX != null)
            {
                GameObject vfx = Instantiate(_collectVFX, transform.position, Quaternion.identity);
                vfx.transform.localScale = Vector3.one * 0.5f;

                // AutoDestroyVFX yoksa 2 saniye sonra yok et
                if (vfx.GetComponent<Core.AutoDestroyVFX>() == null)
                    Destroy(vfx, 2f);
            }

            // SFX — AudioManager varsa kullan, yoksa PlayClipAtPoint
            if (_collectSFX != null)
            {
                if (Core.AudioManager.Instance != null)
                    Core.AudioManager.Instance.PlaySFX(_collectSFX, transform.position);
                else
                    AudioSource.PlayClipAtPoint(_collectSFX, transform.position);
            }

            // Kendini yok et
            Destroy(gameObject);
        }

        // ── 2D KULLANIM İÇİN ──
        // OnTriggerEnter yerine bunu aktif et (OnTriggerEnter'ı sil):
        //
        // private void OnTriggerEnter2D(Collider2D other)
        // {
        //     if (!other.CompareTag("Player")) return;
        //     // ... aynı içerik ...
        // }
    }
}

using UnityEngine;
using TMPro;
using DG.Tweening;

namespace ToySiege.UI
{
    /// <summary>
    /// World-space'te beliren ve yukarı süzülerek kaybolan hasar sayısı.
    ///
    /// Object Pool ile kullanılmak üzere tasarlandı.
    /// DamageNumberSpawner tarafından Get() ile alınır,
    /// animasyon bitince otomatik olarak havuza geri döner.
    ///
    /// KURULUM:
    ///   1) World Space Canvas altına TextMeshPro - Text oluştur
    ///   2) Bu script'i ekle
    ///   3) Prefab yap, DamageNumberSpawner'a referans ver
    /// </summary>
    public class FloatingDamageText : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private TextMeshProUGUI _text;

        [Header("Animasyon")]
        [SerializeField] private float _floatHeight = 1.5f;
        [SerializeField] private float _duration = 0.8f;
        [SerializeField] private float _scaleStart = 0.5f;
        [SerializeField] private float _scalePeak = 1.2f;

        [Header("Renk")]
        [SerializeField] private Color _normalColor = new Color(1f, 0.95f, 0.4f);
        [SerializeField] private Color _criticalColor = new Color(1f, 0.2f, 0.1f);
        [SerializeField] private float _criticalThreshold = 40f;

        private Sequence _sequence;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            if (_text == null)
                _text = GetComponentInChildren<TextMeshProUGUI>();

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>
        /// Hasar sayısını başlat. Pool'dan alındıktan sonra çağır.
        /// </summary>
        public void Play(float damage, Vector3 worldPosition)
        {
            transform.position = worldPosition;

            // Metin ve renk ayarla
            int rounded = Mathf.RoundToInt(damage);
            _text.text = rounded.ToString();
            _text.color = damage >= _criticalThreshold ? _criticalColor : _normalColor;

            // Kameraya bak
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }

            // Animasyon sıfırla
            _sequence?.Kill();
            _canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one * _scaleStart;

            _sequence = DOTween.Sequence();

            // Scale: küçük → büyük → normal
            _sequence.Append(
                transform.DOScale(Vector3.one * _scalePeak, _duration * 0.2f)
                    .SetEase(Ease.OutBack)
            );
            _sequence.Append(
                transform.DOScale(Vector3.one, _duration * 0.3f)
                    .SetEase(Ease.InOutQuad)
            );

            // Yukarı süzülme — paralel
            _sequence.Join(
                transform.DOMoveY(worldPosition.y + _floatHeight, _duration)
                    .SetEase(Ease.OutQuad)
            );

            // Fade out — son %40'ta
            _sequence.Insert(_duration * 0.6f,
                _canvasGroup.DOFade(0f, _duration * 0.4f)
                    .SetEase(Ease.InQuad)
            );

            // Rastgele yatay sapma — üst üste binmesin
            float randomX = Random.Range(-0.3f, 0.3f);
            _sequence.Join(
                transform.DOMoveX(worldPosition.x + randomX, _duration)
                    .SetEase(Ease.OutQuad)
            );

            _sequence.OnComplete(() => gameObject.SetActive(false));
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
        }
    }
}

using UnityEngine;
using TMPro;
using DG.Tweening;
using ToySiege.Core;

namespace ToySiege.UI
{
    /// <summary>
    /// Dalga bilgisini ekranda gösteren UI.
    /// Dalga başlangıcında büyük yazı ile dalga adı gösterir,
    /// kalan düşman sayısını ve dalgalar arası geri sayımı gösterir.
    ///
    /// KURULUM:
    ///   1) Canvas'a 3 TextMeshPro ekle: dalga adı (büyük, ortada), düşman sayısı, geri sayım
    ///   2) Bu script'i ekle, referansları ata
    /// </summary>
    public class WaveUI : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private TextMeshProUGUI _waveAnnounceText;
        [SerializeField] private TextMeshProUGUI _enemyCountText;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private CanvasGroup _announceGroup;

        [Header("Animasyon")]
        [SerializeField] private float _announceDuration = 2.5f;
        [SerializeField] private float _announceScale = 1.3f;

        [Header("Düşman Sayısı")]
        [Tooltip("Sahnedeki düşman sayısını güncelleme aralığı")]
        [SerializeField] private float _countUpdateInterval = 0.5f;

        private Sequence _announceSeq;
        private float _countTimer;

        private void OnEnable()
        {
            if (WaveManager.Instance == null) return;

            WaveManager.Instance.OnWaveStarted += ShowWaveAnnounce;
            WaveManager.Instance.OnWaveCleared += ShowWaveCleared;
            WaveManager.Instance.OnRestCountdown += UpdateCountdown;
            WaveManager.Instance.OnAllWavesCompleted += ShowVictory;
        }

        private void OnDisable()
        {
            if (WaveManager.Instance == null) return;

            WaveManager.Instance.OnWaveStarted -= ShowWaveAnnounce;
            WaveManager.Instance.OnWaveCleared -= ShowWaveCleared;
            WaveManager.Instance.OnRestCountdown -= UpdateCountdown;
            WaveManager.Instance.OnAllWavesCompleted -= ShowVictory;
        }

        private void Start()
        {
            if (_announceGroup != null)
                _announceGroup.alpha = 0f;

            if (_countdownText != null)
                _countdownText.gameObject.SetActive(false);
        }

        private void ShowWaveAnnounce(int index, string name)
        {
            if (_waveAnnounceText == null || _announceGroup == null) return;

            _waveAnnounceText.text = name;

            if (_countdownText != null)
                _countdownText.gameObject.SetActive(false);

            // Animasyon: fade in → bekle → fade out
            _announceSeq?.Kill();
            _announceGroup.alpha = 0f;
            _announceGroup.transform.localScale = Vector3.one * 0.5f;

            _announceSeq = DOTween.Sequence();
            _announceSeq.Append(_announceGroup.DOFade(1f, 0.3f));
            _announceSeq.Join(_announceGroup.transform
                .DOScale(Vector3.one * _announceScale, 0.3f)
                .SetEase(Ease.OutBack));
            _announceSeq.Append(_announceGroup.transform
                .DOScale(Vector3.one, 0.2f)
                .SetEase(Ease.InOutQuad));
            _announceSeq.AppendInterval(_announceDuration);
            _announceSeq.Append(_announceGroup.DOFade(0f, 0.5f));
            _announceSeq.SetUpdate(true);
        }

        private void ShowWaveCleared(int index)
        {
            if (_waveAnnounceText == null || _announceGroup == null) return;

            _waveAnnounceText.text = "Dalga Temizlendi!";

            _announceSeq?.Kill();
            _announceGroup.alpha = 0f;
            _announceGroup.transform.localScale = Vector3.one;

            _announceSeq = DOTween.Sequence();
            _announceSeq.Append(_announceGroup.DOFade(1f, 0.3f));
            _announceSeq.AppendInterval(1.5f);
            _announceSeq.Append(_announceGroup.DOFade(0f, 0.5f));
            _announceSeq.SetUpdate(true);
        }

        private void Update()
        {
            // Sahnedeki gerçek düşman sayısını periyodik olarak say
            _countTimer -= Time.deltaTime;
            if (_countTimer <= 0f)
            {
                _countTimer = _countUpdateInterval;
                int count = GameObject.FindGameObjectsWithTag("Enemy").Length;
                if (_enemyCountText != null)
                    _enemyCountText.text = $"Kalan: {count}";
            }
        }

        private void UpdateCountdown(float seconds)
        {
            if (_countdownText == null) return;

            _countdownText.gameObject.SetActive(true);
            int s = Mathf.CeilToInt(seconds);
            _countdownText.text = $"Sonraki dalga: {s}s";

            if (seconds <= 1f)
                _countdownText.gameObject.SetActive(false);
        }

        private void ShowVictory()
        {
            if (_waveAnnounceText == null || _announceGroup == null) return;

            _waveAnnounceText.text = "ZAFER!";

            _announceSeq?.Kill();
            _announceGroup.alpha = 1f;
            _announceGroup.transform.localScale = Vector3.one * _announceScale;
        }

        private void OnDestroy()
        {
            _announceSeq?.Kill();
        }
    }
}

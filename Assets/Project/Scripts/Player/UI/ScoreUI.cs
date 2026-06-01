using UnityEngine;
using TMPro;
using DG.Tweening;
using ToySiege.Core;

namespace ToySiege.UI
{
    /// <summary>
    /// Skor ve combo bilgisini gösteren UI.
    ///
    /// KURULUM:
    ///   1) Canvas'a TextMeshPro: ScoreText (sol üst), ComboText (orta/sağ)
    ///   2) Bu script'i ekle, referansları ata
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private TextMeshProUGUI _killText;

        [Header("Animasyon")]
        [SerializeField] private float _punchScale = 0.2f;
        [SerializeField] private Color _comboColor = new Color(1f, 0.8f, 0.2f);

        private Tweener _scorePunch;
        private Tweener _comboPunch;
        private Tweener _comboFade;

        private void OnEnable()
        {
            if (ScoreManager.Instance == null) return;

            ScoreManager.Instance.OnScoreChanged += UpdateScore;
            ScoreManager.Instance.OnKill += UpdateKill;
            ScoreManager.Instance.OnComboChanged += UpdateCombo;
        }

        private void OnDisable()
        {
            if (ScoreManager.Instance == null) return;

            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
            ScoreManager.Instance.OnKill -= UpdateKill;
            ScoreManager.Instance.OnComboChanged -= UpdateCombo;
        }

        private void Start()
        {
            UpdateScore(0);
            UpdateCombo(0);

            if (_killText != null)
                _killText.text = "0 Kill";
        }

        private void UpdateScore(int score)
        {
            if (_scoreText == null) return;

            _scoreText.text = score.ToString("N0");

            _scorePunch?.Kill();
            _scoreText.transform.localScale = Vector3.one;
            _scorePunch = _scoreText.transform
                .DOPunchScale(Vector3.one * _punchScale, 0.2f, vibrato: 4)
                .SetUpdate(true);
        }

        private void UpdateKill(int kills, int multiplier)
        {
            if (_killText != null)
                _killText.text = $"{kills} Kill";
        }

        private void UpdateCombo(int combo)
        {
            if (_comboText == null) return;

            if (combo <= 1)
            {
                _comboFade?.Kill();
                _comboFade = _comboText.DOFade(0f, 0.3f).SetUpdate(true);
                return;
            }

            _comboText.text = $"x{combo} COMBO";
            _comboText.color = _comboColor;

            _comboFade?.Kill();
            _comboText.alpha = 1f;

            _comboPunch?.Kill();
            _comboText.transform.localScale = Vector3.one;
            _comboPunch = _comboText.transform
                .DOPunchScale(Vector3.one * _punchScale * 1.5f, 0.25f, vibrato: 6)
                .SetUpdate(true);
        }

        private void OnDestroy()
        {
            _scorePunch?.Kill();
            _comboPunch?.Kill();
            _comboFade?.Kill();
        }
    }
}

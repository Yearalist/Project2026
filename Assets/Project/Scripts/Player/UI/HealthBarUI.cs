using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ToySiege.Player.Health;

namespace ToySiege.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private Image _healthFill;          // yeþil bar
        [SerializeField] private Image _healthFillDelay;     // kýrmýzý gecikme barý
        [SerializeField] private Image _healthBackground;    // arka plan

        [Header("Ayarlar")]
        [SerializeField] private float _delayDuration = 0.5f;
        [SerializeField] private float _delaySpeed = 0.3f;
        [SerializeField] private Color _fullColor = new Color(0.2f, 0.9f, 0.3f);
        [SerializeField] private Color _midColor = new Color(0.9f, 0.8f, 0.2f);
        [SerializeField] private Color _lowColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private float _lowThreshold = 0.3f;
        [SerializeField] private float _midThreshold = 0.6f;

        private Tweener _delayTween;
        private Tweener _punchTween;

        private void OnEnable()
        {
            if (_playerHealth != null)
                _playerHealth.OnHealthChanged += UpdateBar;
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
                _playerHealth.OnHealthChanged -= UpdateBar;
        }

        private void Start()
        {
            // Baþlangýçta full
            if (_healthFill != null) _healthFill.fillAmount = 1f;
            if (_healthFillDelay != null) _healthFillDelay.fillAmount = 1f;
            UpdateColor(1f);
        }

        private void UpdateBar(float current, float max)
        {
            float ratio = current / max;

            // Ana bar — anýnda düþ
            if (_healthFill != null)
                _healthFill.fillAmount = ratio;

            // Renk güncelle
            UpdateColor(ratio);

            // Gecikme barý — yavaþ düþ (hasar hissi)
            _delayTween?.Kill();
            if (_healthFillDelay != null)
            {
                _delayTween = _healthFillDelay
                    .DOFillAmount(ratio, _delaySpeed)
                    .SetDelay(_delayDuration)
                    .SetEase(Ease.InOutSine);
            }

            // Bar sarsýlsýn
            _punchTween?.Kill();
            _punchTween = transform
                .DOPunchScale(Vector3.one * 0.1f, 0.2f, vibrato: 5)
                .SetUpdate(true);
        }

        private void UpdateColor(float ratio)
        {
            if (_healthFill == null) return;

            if (ratio <= _lowThreshold)
                _healthFill.color = _lowColor;
            else if (ratio <= _midThreshold)
                _healthFill.color = Color.Lerp(_lowColor, _midColor,
                    (ratio - _lowThreshold) / (_midThreshold - _lowThreshold));
            else
                _healthFill.color = Color.Lerp(_midColor, _fullColor,
                    (ratio - _midThreshold) / (1f - _midThreshold));
        }

        private void OnDestroy()
        {
            _delayTween?.Kill();
            _punchTween?.Kill();
        }
    }
}
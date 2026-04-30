using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

namespace ToySiege.Player.Health
{
    public class DamageFlashEffect : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private Volume _postProcessVolume;
        [SerializeField] private CanvasGroup _damageOverlay;  // kýrmýzý UI image

        [Header("Ayarlar")]
        [SerializeField] private float _flashDuration = 0.15f;
        [SerializeField] private float _flashAlpha = 0.4f;
        [SerializeField] private float _vignetteHitIntensity = 0.5f;
        [SerializeField] private float _chromaticHitIntensity = 0.6f;
        [SerializeField] private float _hitFreezeTime = 0.05f;

        private Vignette _vignette;
        private ChromaticAberration _chromatic;
        private Tweener _overlayTween;
        private Tweener _vignetteTween;
        private Tweener _chromaticTween;

        private void Awake()
        {
            if (_postProcessVolume != null && _postProcessVolume.profile != null)
            {
                _postProcessVolume.profile.TryGet(out _vignette);
                _postProcessVolume.profile.TryGet(out _chromatic);
            }

            if (_damageOverlay != null)
                _damageOverlay.alpha = 0f;
        }

        private void OnEnable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDamageTaken += HandleDamage;
                _playerHealth.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDamageTaken -= HandleDamage;
                _playerHealth.OnDeath -= HandleDeath;
            }
        }

        private void HandleDamage(float damage)
        {
            // Kýrmýzý overlay flash
            FlashOverlay();

            // Post-process hit efekti
            FlashVignette();
            FlashChromatic();

            // Hit freeze — kýsa an durma hissi
            StartCoroutine(HitFreeze());

            // Kamera shake
            if (GameFeelManager.Instance != null)
            {
                var impulse = GetComponent<Unity.Cinemachine.CinemachineImpulseSource>();
                if (impulse != null)
                    impulse.GenerateImpulse(0.25f);
            }
        }

        private void HandleDeath()
        {
            // Ölümde güçlü overlay
            _overlayTween?.Kill();
            if (_damageOverlay != null)
            {
                _damageOverlay.alpha = 0.6f;
                _damageOverlay.DOFade(0.3f, 1f); // soluk kýrmýzý kalýr
            }
        }

        private void FlashOverlay()
        {
            _overlayTween?.Kill();
            if (_damageOverlay == null) return;

            _damageOverlay.alpha = _flashAlpha;
            _overlayTween = _damageOverlay
                .DOFade(0f, _flashDuration * 3f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        private void FlashVignette()
        {
            _vignetteTween?.Kill();
            if (_vignette == null) return;

            _vignette.intensity.Override(_vignetteHitIntensity);
            _vignetteTween = DOTween.To(
                () => _vignette.intensity.value,
                x => _vignette.intensity.Override(x),
                0f, _flashDuration * 4f
            ).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        private void FlashChromatic()
        {
            _chromaticTween?.Kill();
            if (_chromatic == null) return;

            _chromatic.intensity.Override(_chromaticHitIntensity);
            _chromaticTween = DOTween.To(
                () => _chromatic.intensity.value,
                x => _chromatic.intensity.Override(x),
                0f, _flashDuration * 3f
            ).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        private System.Collections.IEnumerator HitFreeze()
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(_hitFreezeTime);
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            _overlayTween?.Kill();
            _vignetteTween?.Kill();
            _chromaticTween?.Kill();
        }
    }
}
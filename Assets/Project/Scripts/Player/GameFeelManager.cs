using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

namespace ToySiege.Player
{
    public class GameFeelManager : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private Transform _playerModel;
        [SerializeField] private Volume _postProcessVolume;

        [Header("Jump Efektleri")]
        [SerializeField] private ParticleSystem _jumpDust;
        [SerializeField] private ParticleSystem _landingDust;
        [SerializeField] private ParticleSystem _doubleJumpRing;

        [Header("Slide Efektleri")]
        [SerializeField] private ParticleSystem _slideSpeedLines;

        [Header("Squash-Stretch")]
        [SerializeField] private float _jumpStretchY = 1.2f;
        [SerializeField] private float _jumpStretchXZ = 0.8f;
        [SerializeField] private float _landSquashY = 0.7f;
        [SerializeField] private float _landSquashXZ = 1.25f;
        [SerializeField] private float _effectDuration = 0.1f;

        [Header("Kamera Shake")]
        [SerializeField] private float _landShakeStrength = 0.3f;
        [SerializeField] private float _landShakeDuration = 0.2f;
        [SerializeField] private float _doubleJumpShakeStrength = 0.1f;

        [Header("Slide Kamera")]
        [SerializeField] private float _slideTargetFOV = 80f;
        [SerializeField] private float _normalFOV = 60f;
        [SerializeField] private float _fovSpeed = 0.15f;

        [Header("Slide Post-Process")]
        [SerializeField] private float _slideVignetteIntensity = 0.35f;
        [SerializeField] private float _slideChromaticAberration = 0.3f;

        [Header("Cinemachine")]
        [SerializeField] private Unity.Cinemachine.CinemachineImpulseSource _impulseSource;
        [SerializeField] private Unity.Cinemachine.CinemachineCamera _cinemachineCamera;

        [Header("Sprint Kamera")]
        [SerializeField] private float _sprintTargetFOV = 68f;

        private Camera _cam;
        private Tweener _fovTween;
        private Tweener _scaleTween;
        private Tweener _vignetteTween;
        private Tweener _chromaticTween;

        // Post-process referansları
        private Vignette _vignette;
        private ChromaticAberration _chromatic;

        public static GameFeelManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _cam = Camera.main;

            // CinemachineCamera otomatik bul
            if (_cinemachineCamera == null)
                _cinemachineCamera = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();

            // Post-process override'ları bul
            if (_postProcessVolume != null && _postProcessVolume.profile != null)
            {
                _postProcessVolume.profile.TryGet(out _vignette);
                _postProcessVolume.profile.TryGet(out _chromatic);

                // Başlangıçta aktif et ama değer sıfır
                if (_vignette != null)
                {
                    _vignette.active = true;
                    _vignette.intensity.Override(0f);
                }
                if (_chromatic != null)
                {
                    _chromatic.active = true;
                    _chromatic.intensity.Override(0f);
                }
            }
        }

        // ══════════════════════════════
        //  JUMP
        // ══════════════════════════════
        public void OnJump()
        {
            if (_jumpDust != null) _jumpDust.Play();
            PlayScale(
                new Vector3(_jumpStretchXZ, _jumpStretchY, _jumpStretchXZ),
                _effectDuration
            );
        }

        // ══════════════════════════════
        //  DOUBLE JUMP
        // ══════════════════════════════
        public void OnDoubleJump()
        {
            if (_doubleJumpRing != null) _doubleJumpRing.Play();
            PlayScale(
                new Vector3(_jumpStretchXZ, _jumpStretchY, _jumpStretchXZ),
                _effectDuration * 0.8f
            );
            if (_impulseSource != null)
                _impulseSource.GenerateImpulse(_doubleJumpShakeStrength);
        }

        // ══════════════════════════════
        //  LANDING
        // ══════════════════════════════
        public void OnLanding(float fallSpeed)
        {
            float intensity = Mathf.Clamp01(Mathf.Abs(fallSpeed) / 15f);
            if (intensity < 0.2f) return;

            if (_landingDust != null)
            {
                var main = _landingDust.main;
                main.startSpeedMultiplier = 2f + intensity * 4f;
                _landingDust.Play();
            }

            PlayScale(
                Vector3.Lerp(Vector3.one,
                    new Vector3(_landSquashXZ, _landSquashY, _landSquashXZ),
                    intensity),
                _effectDuration
            );

            if (_impulseSource != null)
                _impulseSource.GenerateImpulse(_landShakeStrength * intensity);

            // Landing'de kısa chromatic aberration flash
            FlashChromatic(0.4f * intensity, 0.3f);
        }

        // ══════════════════════════════
        //  SLIDE
        // ══════════════════════════════
        public void OnSlideStart()
        {
            // Speed lines
            if (_slideSpeedLines != null) _slideSpeedLines.Play();

            // FOV artışı
            AnimateFOV(_slideTargetFOV, _fovSpeed, Ease.OutBack);

            // Vignette — tünel görüşü
            _vignetteTween?.Kill();
            if (_vignette != null)
                _vignetteTween = DOTween.To(
                    () => _vignette.intensity.value,
                    x => _vignette.intensity.Override(x),
                    _slideVignetteIntensity,
                    _fovSpeed
                ).SetEase(Ease.OutQuad);

            // Chromatic aberration — hız hissi
            _chromaticTween?.Kill();
            if (_chromatic != null)
                _chromaticTween = DOTween.To(
                    () => _chromatic.intensity.value,
                    x => _chromatic.intensity.Override(x),
                    _slideChromaticAberration,
                    _fovSpeed
                ).SetEase(Ease.OutQuad);
        }

        public void OnSlideEnd()
        {
            // Speed lines durdur
            if (_slideSpeedLines != null) _slideSpeedLines.Stop();

            // FOV normale
            AnimateFOV(_normalFOV, _fovSpeed * 1.5f, Ease.InOutQuad);

            // Vignette sıfırla
            _vignetteTween?.Kill();
            if (_vignette != null)
                _vignetteTween = DOTween.To(
                    () => _vignette.intensity.value,
                    x => _vignette.intensity.Override(x),
                    0f,
                    _fovSpeed * 1.5f
                ).SetEase(Ease.InOutQuad);

            // Chromatic sıfırla
            _chromaticTween?.Kill();
            if (_chromatic != null)
                _chromaticTween = DOTween.To(
                    () => _chromatic.intensity.value,
                    x => _chromatic.intensity.Override(x),
                    0f,
                    _fovSpeed * 1.5f
                ).SetEase(Ease.InOutQuad);
        }

        // ══════════════════════════════
        //  YARDIMCI
        // ══════════════════════════════
        private void PlayScale(Vector3 target, float duration)
        {
            if (_playerModel == null) return;
            _scaleTween?.Kill();
            _playerModel.localScale = Vector3.one;
            _scaleTween = _playerModel
                .DOScale(target, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _scaleTween = _playerModel
                        .DOScale(Vector3.one, duration * 2.5f)
                        .SetEase(Ease.OutBack);
                });
        }

        /// <summary>
        /// FOV'u Cinemachine üzerinden değiştirir.
        /// Cinemachine yoksa fallback olarak Camera.fieldOfView kullanır.
        /// </summary>
        private void AnimateFOV(float targetFOV, float duration, Ease ease = Ease.OutQuad)
        {
            _fovTween?.Kill();

            if (_cinemachineCamera != null)
            {
                // Cinemachine 3.x — Lens.FieldOfView üzerinden
                _fovTween = DOTween.To(
                    () => _cinemachineCamera.Lens.FieldOfView,
                    x =>
                    {
                        var lens = _cinemachineCamera.Lens;
                        lens.FieldOfView = x;
                        _cinemachineCamera.Lens = lens;
                    },
                    targetFOV,
                    duration
                ).SetEase(ease);
            }
            else if (_cam != null)
            {
                _fovTween = _cam.DOFieldOfView(targetFOV, duration).SetEase(ease);
            }
        }

        private void FlashChromatic(float intensity, float duration)
        {
            if (_chromatic == null) return;
            _chromaticTween?.Kill();
            _chromatic.intensity.Override(intensity);
            _chromaticTween = DOTween.To(
                () => _chromatic.intensity.value,
                x => _chromatic.intensity.Override(x),
                0f,
                duration
            ).SetEase(Ease.OutQuad);
        }

        private void OnDestroy()
        {
            _fovTween?.Kill();
            _scaleTween?.Kill();
            _vignetteTween?.Kill();
            _chromaticTween?.Kill();
        }

        public void OnSprintStart()
        {
            if (_isAiming) return; // ADS aktifse sprint FOV uygulanmasın

            AnimateFOV(_sprintTargetFOV, 0.3f);
        }

        public void OnSprintEnd()
        {
            if (_isAiming) return; // ADS aktifse sprint FOV reset yapmasın

            // Slide aktifse dokunma
            if (_slideSpeedLines != null && _slideSpeedLines.isPlaying) return;

            AnimateFOV(_normalFOV, 0.4f, Ease.InOutQuad);
        }

        // ══════════════════════════════
        //  ADS (Nişan Alma)
        // ══════════════════════════════

        [Header("ADS")]
        [SerializeField] private float _adsFOV = 40f;
        [SerializeField] private float _adsTransitionSpeed = 0.15f;

        private bool _isAiming;

        /// <summary>
        /// PlayerCombat tarafından çağrılır.
        /// </summary>
        public void SetAiming(bool aiming)
        {
            _isAiming = aiming;

            float targetFOV = aiming ? _adsFOV : _normalFOV;
            AnimateFOV(targetFOV, _adsTransitionSpeed, aiming ? Ease.OutQuad : Ease.InOutQuad);

            // ADS sırasında vignette efekti
            _vignetteTween?.Kill();
            if (_vignette != null)
            {
                _vignetteTween = DOTween.To(
                    () => _vignette.intensity.value,
                    x => _vignette.intensity.Override(x),
                    aiming ? 0.25f : 0f,
                    _adsTransitionSpeed
                ).SetEase(Ease.OutQuad);
            }
        }
    }
}
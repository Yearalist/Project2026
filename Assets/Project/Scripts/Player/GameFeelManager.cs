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
            _fovTween?.Kill();
            if (_cam != null)
                _fovTween = _cam.DOFieldOfView(_slideTargetFOV, _fovSpeed)
                    .SetEase(Ease.OutBack);

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
            _fovTween?.Kill();
            if (_cam != null)
                _fovTween = _cam.DOFieldOfView(_normalFOV, _fovSpeed * 1.5f)
                    .SetEase(Ease.InOutQuad);

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
            _fovTween?.Kill();
            if (_cam != null)
                _fovTween = _cam.DOFieldOfView(_sprintTargetFOV, 0.3f)
                    .SetEase(Ease.OutQuad);
        }

        public void OnSprintEnd()
        {
            // Slide aktifse dokunma
            if (_slideSpeedLines != null && _slideSpeedLines.isPlaying) return;

            _fovTween?.Kill();
            if (_cam != null)
                _fovTween = _cam.DOFieldOfView(_normalFOV, 0.4f)
                    .SetEase(Ease.InOutQuad);
        }
    }
}
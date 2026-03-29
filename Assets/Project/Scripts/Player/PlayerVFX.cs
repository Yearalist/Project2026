using UnityEngine;

namespace ToySiege.Player
{
    public class PlayerVFX : MonoBehaviour
    {
        [Header("══ AYAK TOZU ══")]
        [SerializeField] private ParticleSystem _footDust;
        [SerializeField] private float _walkEmissionRate = 8f;
        [SerializeField] private float _sprintEmissionRate = 20f;

        [Header("══ SLIDE PATLAMA ══")]
        [SerializeField] private ParticleSystem _slideBurst;
        [SerializeField] private ParticleSystem _slideTrail;

        private bool _footDustPlaying;

        private void Awake()
        {
            // Sadece varsa durdur — yoksa sessizce geç
            if (_footDust != null)
            {
                _footDust.Stop();
                var em = _footDust.emission;
                em.rateOverTime = 0f;
            }

            if (_slideBurst != null)
                _slideBurst.Stop();

            if (_slideTrail != null)
                _slideTrail.Stop();
        }

        // ══════════════════════════════════════════
        // AYAK TOZU
        // ══════════════════════════════════════════

        public void StartWalkDust()
        {
            SetFootDust(_walkEmissionRate);
        }

        public void StartSprintDust()
        {
            SetFootDust(_sprintEmissionRate);
        }

        public void StopFootDust()
        {
            // _footDust atanmamışsa hiçbir şey yapma — HATA VERMEZ
            if (_footDust == null) return;

            var emission = _footDust.emission;
            emission.rateOverTime = 0f;
            _footDustPlaying = false;
        }

        private void SetFootDust(float rate)
        {
            if (_footDust == null) return;

            var emission = _footDust.emission;
            emission.rateOverTime = rate;

            if (!_footDustPlaying)
            {
                _footDust.Play();
                _footDustPlaying = true;
            }
        }

        // ══════════════════════════════════════════
        // SLIDE
        // ══════════════════════════════════════════

        public void PlaySlideBurst()
        {
            if (_slideBurst != null)
            {
                _slideBurst.Stop();
                _slideBurst.Clear();
                _slideBurst.Play();
            }

            if (_slideTrail != null)
                _slideTrail.Play();

            StopFootDust();
        }

        public void StopSlideTrail()
        {
            if (_slideTrail != null)
                _slideTrail.Stop();
        }
    }
}
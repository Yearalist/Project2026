
using UnityEngine;

namespace ToySiege.Player
{
    public class PlayerVFX : MonoBehaviour
    {
        // ══════════════════════════════════════════
        // INSPECTOR — Particle System referansları
        // ══════════════════════════════════════════

        [Header("══ AYAK TOZU ══")]
        [Tooltip("Player child'ı olarak ayak hizasında oluşturulan Particle System")]
        [SerializeField] private ParticleSystem _footDust;

        [Tooltip("Yürürken saniyede kaç parçacık çıkar")]
        [SerializeField] private float _walkEmissionRate = 8f;

        [Tooltip("Koşarken saniyede kaç parçacık çıkar (daha fazla = daha dramatik)")]
        [SerializeField] private float _sprintEmissionRate = 20f;

        [Header("══ SLIDE PATLAMA ══")]
        [Tooltip("Slide başladığında bir kez patlayan Particle System")]
        [SerializeField] private ParticleSystem _slideBurst;

        [Tooltip("Slide devam ederken sürekli çıkan iz efekti (opsiyonel)")]
        [SerializeField] private ParticleSystem _slideTrail;

        // ══════════════════════════════════════════
        // PRIVATE
        // ══════════════════════════════════════════

        private ParticleSystem.EmissionModule _footDustEmission;
        private bool _footDustActive;

        private void Awake()
        {
            if (_footDust != null)
            {
                _footDustEmission = _footDust.emission;
                _footDustEmission.rateOverTime = 0f;
                _footDust.Stop();
            }

            if (_slideBurst != null)
                _slideBurst.Stop();

            if (_slideTrail != null)
                _slideTrail.Stop();
        }

        // ══════════════════════════════════════════
        // AYAK TOZU — State'ler tarafından çağrılır
        // ══════════════════════════════════════════

        /// <summary>
        /// Yürürken ayak tozu başlat (düşük yoğunluk).
        /// WalkState.Enter()'da çağrılır.
        /// </summary>
        public void StartWalkDust()
        {
            SetFootDust(_walkEmissionRate);
        }

        /// <summary>
        /// Koşarken ayak tozu (yüksek yoğunluk).
        /// SprintState.Enter()'da çağrılır.
        /// </summary>
        public void StartSprintDust()
        {
            SetFootDust(_sprintEmissionRate);
        }

        /// <summary>
        /// Ayak tozunu durdur.
        /// Idle veya havadaki state'lerin Enter()'ında çağrılır.
        /// </summary>
        public void StopFootDust()
        {
            if (_footDust == null) return;
            _footDustEmission.rateOverTime = 0f;
            _footDustActive = false;
            // Stop yerine emission'ı sıfırlıyoruz — mevcut parçacıklar 
            // doğal ömürlerini tamamlayıp kaybolur, ani kesme olmaz
        }

        private void SetFootDust(float rate)
        {
            if (_footDust == null) return;

            _footDustEmission.rateOverTime = rate;

            if (!_footDustActive)
            {
                _footDust.Play();
                _footDustActive = true;
            }
        }

        public void PlaySlideBurst()
        {
            // Ana patlama — tek seferlik
            if (_slideBurst != null)
            {
                _slideBurst.Stop();
                _slideBurst.Clear();
                _slideBurst.Play();
            }

            // İz efekti — slide boyunca devam eder
            if (_slideTrail != null)
            {
                _slideTrail.Play();
            }

            // Ayak tozunu kapat (slide kendi efektini kullanır)
            StopFootDust();
        }

        /// <summary>
        /// Slide bittiğinde iz efektini durdurur.
        /// SlideState.Exit()'de çağrılır.
        /// </summary>
        public void StopSlideTrail()
        {
            if (_slideTrail != null)
                _slideTrail.Stop();
        }
    }
}
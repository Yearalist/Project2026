using System;
using UnityEngine;

namespace ToySiege.Core
{
    /// <summary>
    /// Skor ve kill takip sistemi — Singleton.
    ///
    /// Düşman öldüğünde skor artar, combo sistemi ile arka arkaya
    /// yapılan kill'ler bonus puan verir. WaveManager ile entegre
    /// çalışarak dalga sonu bonus puanı verir.
    ///
    /// KULLANIM:
    ///   ScoreManager.Instance.AddKill(points);
    ///   ScoreManager.Instance.TotalScore;
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        // ── Event'ler — UI dinleyecek ──
        /// <summary> Skor değiştiğinde (totalScore) </summary>
        public event Action<int> OnScoreChanged;
        /// <summary> Kill yapıldığında (killCount, comboMultiplier) </summary>
        public event Action<int, int> OnKill;
        /// <summary> Combo değiştiğinde (comboCount) </summary>
        public event Action<int> OnComboChanged;

        [Header("Puan Ayarları")]
        [SerializeField] private int _baseKillScore = 100;
        [SerializeField] private int _waveClearBonus = 500;

        [Header("Combo")]
        [Tooltip("Combo sıfırlanmadan önce geçmesi gereken süre (saniye)")]
        [SerializeField] private float _comboTimeout = 3f;
        [Tooltip("Combo başına çarpan artışı")]
        [SerializeField] private float _comboMultiplierStep = 0.5f;
        [Tooltip("Maksimum combo çarpanı")]
        [SerializeField] private int _maxComboMultiplier = 5;

        public int TotalScore { get; private set; }
        public int TotalKills { get; private set; }
        public int CurrentCombo { get; private set; }
        public int ComboMultiplier => Mathf.Min(1 + Mathf.FloorToInt(CurrentCombo * _comboMultiplierStep), _maxComboMultiplier);

        private float _lastKillTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // WaveManager varsa dalga temizleme bonusu bağla
            if (WaveManager.Instance != null)
                WaveManager.Instance.OnWaveCleared += HandleWaveCleared;
        }

        private void Update()
        {
            // Combo timeout kontrolü
            if (CurrentCombo > 0 && Time.time - _lastKillTime > _comboTimeout)
            {
                CurrentCombo = 0;
                OnComboChanged?.Invoke(0);
            }
        }

        /// <summary>
        /// Kill kaydeder ve skor ekler.
        /// bonusPoints: düşman tipine göre ekstra puan (opsiyonel).
        /// </summary>
        public void AddKill(int bonusPoints = 0)
        {
            TotalKills++;
            _lastKillTime = Time.time;

            // Combo artır
            CurrentCombo++;
            OnComboChanged?.Invoke(CurrentCombo);

            // Puan hesapla
            int points = (_baseKillScore + bonusPoints) * ComboMultiplier;
            TotalScore += points;

            OnKill?.Invoke(TotalKills, ComboMultiplier);
            OnScoreChanged?.Invoke(TotalScore);

            Debug.Log($"<color=yellow>[Score] Kill #{TotalKills} | +{points} puan (x{ComboMultiplier} combo) | Toplam: {TotalScore}</color>");
        }

        private void HandleWaveCleared(int waveIndex)
        {
            int bonus = _waveClearBonus * (waveIndex + 1);
            TotalScore += bonus;
            OnScoreChanged?.Invoke(TotalScore);

            Debug.Log($"<color=green>[Score] Dalga {waveIndex + 1} bonus: +{bonus} | Toplam: {TotalScore}</color>");
        }

        public void ResetScore()
        {
            TotalScore = 0;
            TotalKills = 0;
            CurrentCombo = 0;
            OnScoreChanged?.Invoke(0);
            OnComboChanged?.Invoke(0);
        }

        private void OnDestroy()
        {
            if (WaveManager.Instance != null)
                WaveManager.Instance.OnWaveCleared -= HandleWaveCleared;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToySiege.Core
{
    /// <summary>
    /// Dalga bazlı düşman spawn yöneticisi.
    ///
    /// WaveConfig ScriptableObject'ten dalga verilerini okur,
    /// belirlenen spawn noktalarından düşmanları yaratır,
    /// tüm düşmanlar ölünce bir sonraki dalgaya geçer.
    ///
    /// KURULUM:
    ///   1) Sahneye boş GameObject: WaveManager
    ///   2) WaveManager script'ini ekle
    ///   3) WaveConfig ScriptableObject oluştur (Create → ToySiege → Wave Config)
    ///   4) Sahnede boş GameObject'ler oluştur → _spawnPoints dizisine ata
    ///   5) Play!
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        // ── Event'ler — UI dinleyecek ──
        /// <summary> Yeni dalga başladığında (waveIndex, waveName) </summary>
        public event Action<int, string> OnWaveStarted;
        /// <summary> Dalga temizlendiğinde (waveIndex) </summary>
        public event Action<int> OnWaveCleared;
        /// <summary> Dalgalar arası mola (kalanSaniye) — her saniye tetiklenir </summary>
        public event Action<float> OnRestCountdown;
        /// <summary> Tüm dalgalar bitti </summary>
        public event Action OnAllWavesCompleted;
        /// <summary> Hayatta kalan düşman sayısı değiştiğinde (kalan) </summary>
        public event Action<int> OnEnemyCountChanged;

        [Header("Konfigürasyon")]
        [SerializeField] private WaveConfig _config;

        [Header("Spawn Noktaları")]
        [SerializeField] private Transform[] _spawnPoints;

        [Header("Ayarlar")]
        [Tooltip("İlk dalga başlamadan önce bekleme süresi")]
        [SerializeField] private float _initialDelay = 3f;
        [Tooltip("Spawn noktasında rastgele dağılım yarıçapı")]
        [SerializeField] private float _spawnRadius = 2f;

        public int CurrentWaveIndex { get; private set; }
        public int TotalWaves => _config != null && _config.Waves != null ? _config.Waves.Length : 0;
        public bool IsActive { get; private set; }
        public int AliveEnemyCount => _aliveEnemies.Count;

        private readonly List<GameObject> _aliveEnemies = new();
        private int _loopCount;
        private Coroutine _waveRoutine;

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
            if (_config == null)
            {
                Debug.LogError("[WaveManager] WaveConfig atanmamış!");
                return;
            }

            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                Debug.LogError("[WaveManager] Spawn noktası yok!");
                return;
            }

            StartWaves();
        }

        /// <summary>
        /// Dalga sistemini başlat veya yeniden başlat.
        /// </summary>
        public void StartWaves()
        {
            if (_waveRoutine != null)
                StopCoroutine(_waveRoutine);

            CurrentWaveIndex = 0;
            _loopCount = 0;
            IsActive = true;
            _waveRoutine = StartCoroutine(WaveLoop());
        }

        public void StopWaves()
        {
            if (_waveRoutine != null)
                StopCoroutine(_waveRoutine);

            IsActive = false;
        }

        private IEnumerator WaveLoop()
        {
            yield return new WaitForSeconds(_initialDelay);

            while (true)
            {
                if (_config.Waves == null || _config.Waves.Length == 0)
                    yield break;

                // Mevcut dalga
                int waveIdx = CurrentWaveIndex % _config.Waves.Length;
                var wave = _config.Waves[waveIdx];

                string waveName = $"{wave.WaveName} {CurrentWaveIndex + 1}";
                OnWaveStarted?.Invoke(CurrentWaveIndex, waveName);
                Debug.Log($"<color=cyan>[Wave] {waveName} başlıyor!</color>");

                // Düşmanları spawn et
                yield return StartCoroutine(SpawnWave(wave));

                // Tüm düşmanlar ölene kadar bekle
                while (_aliveEnemies.Count > 0)
                {
                    // Yok edilmiş objeleri temizle
                    CleanDeadEnemies();
                    yield return new WaitForSeconds(0.5f);
                }

                OnWaveCleared?.Invoke(CurrentWaveIndex);
                Debug.Log($"<color=green>[Wave] {waveName} temizlendi!</color>");

                CurrentWaveIndex++;

                // Tüm dalgalar bitti mi?
                if (CurrentWaveIndex >= _config.Waves.Length)
                {
                    if (_config.LoopWaves)
                    {
                        _loopCount++;
                        Debug.Log($"<color=yellow>[Wave] Dalgalar tekrar başlıyor! Loop: {_loopCount}</color>");
                    }
                    else
                    {
                        OnAllWavesCompleted?.Invoke();
                        Debug.Log("<color=green>[Wave] TÜM DALGALAR TAMAMLANDI!</color>");
                        IsActive = false;
                        yield break;
                    }
                }

                // Dalgalar arası mola
                float rest = wave.RestTime;
                while (rest > 0f)
                {
                    OnRestCountdown?.Invoke(rest);
                    yield return new WaitForSeconds(1f);
                    rest -= 1f;
                }
            }
        }

        private IEnumerator SpawnWave(WaveDefinition wave)
        {
            if (wave.Entries == null) yield break;

            // Zorluk çarpanı — loop sayısına göre artar
            float healthMult = Mathf.Pow(_config.HealthMultiplierPerWave, CurrentWaveIndex);
            int extraCount = _config.ExtraEnemiesPerWave * _loopCount;

            foreach (var entry in wave.Entries)
            {
                if (entry.EnemyPrefab == null) continue;

                int totalCount = entry.Count + extraCount;

                for (int i = 0; i < totalCount; i++)
                {
                    // Rastgele spawn noktası seç
                    Transform spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];
                    Vector3 pos = spawnPoint.position
                        + UnityEngine.Random.insideUnitSphere * _spawnRadius;
                    pos.y = spawnPoint.position.y; // Y'yi spawn noktasında tut

                    // NavMesh üzerinde geçerli pozisyon bul
                    if (UnityEngine.AI.NavMesh.SamplePosition(pos, out var hit, _spawnRadius * 2f, UnityEngine.AI.NavMesh.AllAreas))
                        pos = hit.position;

                    GameObject enemy = Instantiate(entry.EnemyPrefab, pos, Quaternion.identity);

                    // Zorluk çarpanı uygula — sağlık artışı
                    // EnemyController varsa HP scale
                    var controller = enemy.GetComponent<Enemy.EnemyController>();
                    if (controller != null && healthMult > 1f)
                    {
                        // Config runtime'da değiştirilmemeli — bunun yerine TakeDamage'da
                        // scale uygulanabilir. Şimdilik basit tutalım.
                    }

                    // PoliceCarEnemy varsa
                    var car = enemy.GetComponent<Enemy.PoliceCarEnemy>();
                    if (car != null && healthMult > 1f)
                    {
                        car.maxHealth *= healthMult;
                    }

                    _aliveEnemies.Add(enemy);
                    OnEnemyCountChanged?.Invoke(_aliveEnemies.Count);

                    if (entry.SpawnInterval > 0f)
                        yield return new WaitForSeconds(entry.SpawnInterval);
                }
            }
        }

        private void CleanDeadEnemies()
        {
            int before = _aliveEnemies.Count;
            _aliveEnemies.RemoveAll(e => e == null);

            if (_aliveEnemies.Count != before)
                OnEnemyCountChanged?.Invoke(_aliveEnemies.Count);
        }

        private void OnDestroy()
        {
            if (_waveRoutine != null)
                StopCoroutine(_waveRoutine);
        }
    }
}

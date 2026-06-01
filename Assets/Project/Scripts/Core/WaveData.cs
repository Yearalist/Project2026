using System;
using UnityEngine;

namespace ToySiege.Core
{
    /// <summary>
    /// Tek bir dalganın konfigürasyonu.
    /// WaveConfig ScriptableObject'in içinde dizi olarak tutulur.
    /// </summary>
    [Serializable]
    public class WaveEntry
    {
        [Tooltip("Spawn edilecek düşman prefab'ı")]
        public GameObject EnemyPrefab;

        [Tooltip("Bu prefab'dan kaç adet spawn edilecek")]
        public int Count = 3;

        [Tooltip("Her spawn arası bekleme süresi (saniye)")]
        public float SpawnInterval = 0.5f;
    }

    /// <summary>
    /// Bir dalganın tüm bilgilerini tutan veri sınıfı.
    /// </summary>
    [Serializable]
    public class WaveDefinition
    {
        public string WaveName = "Dalga";

        [Tooltip("Bu dalgada spawn edilecek düşman grupları")]
        public WaveEntry[] Entries;

        [Tooltip("Bu dalga bittikten sonra bir sonraki dalgaya kadar bekleme (saniye)")]
        public float RestTime = 5f;
    }

    /// <summary>
    /// Tüm dalga verilerini tutan ScriptableObject.
    ///
    /// KULLANIM:
    ///   Project panelinde sağ tık → Create → ToySiege → Wave Config
    ///   Dalga sayısını ve her dalganın düşman kompozisyonunu Inspector'da ayarla.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "ToySiege/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        [Header("Dalgalar")]
        public WaveDefinition[] Waves;

        [Header("Zorluk Artışı")]
        [Tooltip("Her dalga sonrası düşman sağlığına eklenen çarpan (1.0 = değişim yok)")]
        public float HealthMultiplierPerWave = 1.1f;

        [Tooltip("Her dalga sonrası düşman sayısına eklenen bonus")]
        public int ExtraEnemiesPerWave = 1;

        [Tooltip("Tüm dalgalar bittikten sonra tekrar başlasın mı (endless mode)")]
        public bool LoopWaves = true;
    }
}

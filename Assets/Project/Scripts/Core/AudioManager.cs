using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ToySiege.Core
{
    /// <summary>
    /// Merkezi ses yöneticisi — Singleton.
    ///
    /// SFX, müzik ve UI sesleri için ayrı AudioSource havuzları kullanır.
    /// Object Pool mantığıyla çalışır: her PlaySFX çağrısı havuzdan bir
    /// AudioSource alır, ses bitince geri koyar.
    ///
    /// KULLANIM:
    ///   AudioManager.Instance.PlaySFX(clipRef, position);
    ///   AudioManager.Instance.PlayMusic(musicClip);
    ///   AudioManager.Instance.SetSFXVolume(0.5f);
    ///
    /// KURULUM:
    ///   1) Sahneye boş GameObject ekle → AudioManager ata
    ///   2) (Opsiyonel) AudioMixer ata → _mixer'a referans ver
    ///   3) DontDestroyOnLoad aktif — sahne geçişlerinde kaybolmaz
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer (Opsiyonel)")]
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;
        [SerializeField] private AudioMixerGroup _musicMixerGroup;

        [Header("Müzik")]
        [SerializeField] private AudioSource _musicSource;

        [Header("SFX Havuzu")]
        [SerializeField] private int _sfxPoolSize = 16;

        [Header("Ses Seviyeleri")]
        [Range(0f, 1f)] [SerializeField] private float _masterVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float _musicVolume = 0.5f;

        private readonly Queue<AudioSource> _sfxPool = new();
        private readonly List<AudioSource> _activeSources = new();

        // ══════════════════════════════
        //  LIFECYCLE
        // ══════════════════════════════

        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Müzik kaynağı yoksa oluştur
            if (_musicSource == null)
            {
                var musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                _musicSource = musicObj.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }

            if (_musicMixerGroup != null)
                _musicSource.outputAudioMixerGroup = _musicMixerGroup;

            _musicSource.volume = _musicVolume * _masterVolume;

            // SFX havuzunu doldur
            for (int i = 0; i < _sfxPoolSize; i++)
            {
                _sfxPool.Enqueue(CreateSFXSource());
            }
        }

        private void Update()
        {
            // Biten SFX source'larını havuza geri koy
            for (int i = _activeSources.Count - 1; i >= 0; i--)
            {
                if (_activeSources[i] == null)
                {
                    _activeSources.RemoveAt(i);
                    continue;
                }

                if (!_activeSources[i].isPlaying)
                {
                    ReturnSFXSource(_activeSources[i]);
                    _activeSources.RemoveAt(i);
                }
            }
        }

        // ══════════════════════════════
        //  SFX
        // ══════════════════════════════

        /// <summary>
        /// 2D SFX çal (UI sesleri, genel efektler).
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;

            var source = GetSFXSource();
            source.spatialBlend = 0f; // 2D
            source.clip = clip;
            source.volume = _sfxVolume * _masterVolume * volumeScale;
            source.Play();
            _activeSources.Add(source);
        }

        /// <summary>
        /// 3D SFX çal (silah sesi, patlama, ayak sesi).
        /// Pozisyon belirtilir, mesafeye göre ses azalır.
        /// </summary>
        public void PlaySFX(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (clip == null) return;

            var source = GetSFXSource();
            source.transform.position = position;
            source.spatialBlend = 1f; // 3D
            source.clip = clip;
            source.volume = _sfxVolume * _masterVolume * volumeScale;
            source.Play();
            _activeSources.Add(source);
        }

        /// <summary>
        /// Rastgele bir klip seçerek çal — aynı sesin tekrar etmesini önler.
        /// </summary>
        public void PlayRandomSFX(AudioClip[] clips, Vector3 position, float volumeScale = 1f)
        {
            if (clips == null || clips.Length == 0) return;
            PlaySFX(clips[Random.Range(0, clips.Length)], position, volumeScale);
        }

        // ══════════════════════════════
        //  MÜZİK
        // ══════════════════════════════

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.volume = _musicVolume * _masterVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        public void PauseMusic() => _musicSource.Pause();
        public void ResumeMusic() => _musicSource.UnPause();

        // ══════════════════════════════
        //  SES SEVİYELERİ
        // ══════════════════════════════

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            _musicSource.volume = _musicVolume * _masterVolume;
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            _musicSource.volume = _musicVolume * _masterVolume;
        }

        // ══════════════════════════════
        //  POOL YARDIMCI
        // ══════════════════════════════

        private AudioSource GetSFXSource()
        {
            if (_sfxPool.Count > 0)
            {
                var source = _sfxPool.Dequeue();
                source.gameObject.SetActive(true);
                return source;
            }

            // Havuz boşsa yeni oluştur
            return CreateSFXSource();
        }

        private void ReturnSFXSource(AudioSource source)
        {
            source.clip = null;
            source.gameObject.SetActive(false);
            _sfxPool.Enqueue(source);
        }

        private AudioSource CreateSFXSource()
        {
            var obj = new GameObject("SFX_Source");
            obj.transform.SetParent(transform);

            var source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 1f;
            source.minDistance = 2f;
            source.maxDistance = 30f;
            source.rolloffMode = AudioRolloffMode.Linear;

            if (_sfxMixerGroup != null)
                source.outputAudioMixerGroup = _sfxMixerGroup;

            obj.SetActive(false);
            return source;
        }
    }
}

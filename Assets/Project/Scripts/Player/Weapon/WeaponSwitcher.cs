using System;
using UnityEngine;

namespace ToySiege.Player.Combat
{
    /// <summary>
    /// Birden fazla silah arasında geçiş yapan sistem.
    ///
    /// Silahlar bu GameObject'in child'ları olarak yerleştirilir.
    /// Aktif olmayan silahlar deaktif edilir.
    /// Scroll wheel veya sayı tuşları (1-9) ile değiştirilir.
    ///
    /// KURULUM:
    ///   1) Oyuncunun elindeki/silah tuttuğu objede bu script olmalı
    ///   2) Her silah (Weapon component'li) bu objenin child'ı olarak yerleştirilir
    ///   3) PlayerCombat'a bu script'in referansını ver (veya otomatik bulur)
    /// </summary>
    public class WeaponSwitcher : MonoBehaviour
    {
        /// <summary> Silah değiştiğinde (yeniSilah) </summary>
        public event Action<Weapon> OnWeaponChanged;

        [Header("Ayarlar")]
        [Tooltip("Silah değiştirme scroll hassasiyeti")]
        [SerializeField] private float _scrollThreshold = 0.1f;

        [Header("Başlangıç")]
        [Tooltip("Oyun başında aktif olacak silah indexi")]
        [SerializeField] private int _startWeaponIndex = 0;

        private Weapon[] _weapons;
        private int _currentIndex;

        public Weapon CurrentWeapon => _weapons != null && _weapons.Length > 0
            ? _weapons[_currentIndex]
            : null;

        public int WeaponCount => _weapons != null ? _weapons.Length : 0;
        public int CurrentIndex => _currentIndex;

        private void Awake()
        {
            // Child'lardaki tüm Weapon component'lerini topla
            _weapons = GetComponentsInChildren<Weapon>(includeInactive: true);

            if (_weapons.Length == 0)
            {
                Debug.LogWarning("[WeaponSwitcher] Hiç silah bulunamadı!");
                return;
            }

            // Hepsini deaktif et
            foreach (var w in _weapons)
                w.gameObject.SetActive(false);

            // Başlangıç silahını aktif et
            _currentIndex = Mathf.Clamp(_startWeaponIndex, 0, _weapons.Length - 1);
            _weapons[_currentIndex].gameObject.SetActive(true);
        }

        private void Update()
        {
            if (_weapons == null || _weapons.Length <= 1) return;

            // Scroll wheel ile değiştir
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > _scrollThreshold)
                SwitchWeapon(_currentIndex + 1);
            else if (scroll < -_scrollThreshold)
                SwitchWeapon(_currentIndex - 1);

            // Sayı tuşları ile değiştir (1-9)
            for (int i = 0; i < Mathf.Min(_weapons.Length, 9); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SwitchWeapon(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Belirtilen index'teki silaha geç.
        /// </summary>
        public void SwitchWeapon(int index)
        {
            if (_weapons == null || _weapons.Length == 0) return;

            // Wrap around
            index = ((index % _weapons.Length) + _weapons.Length) % _weapons.Length;

            if (index == _currentIndex) return;

            // Mevcut silahı kapat
            _weapons[_currentIndex].gameObject.SetActive(false);

            // Yeni silahı aç
            _currentIndex = index;
            _weapons[_currentIndex].gameObject.SetActive(true);

            OnWeaponChanged?.Invoke(_weapons[_currentIndex]);

            Debug.Log($"<color=yellow>[WeaponSwitcher] Silah değişti: {_weapons[_currentIndex].name} ({_currentIndex + 1}/{_weapons.Length})</color>");
        }

        /// <summary>
        /// Sonraki silaha geç.
        /// </summary>
        public void NextWeapon() => SwitchWeapon(_currentIndex + 1);

        /// <summary>
        /// Önceki silaha geç.
        /// </summary>
        public void PreviousWeapon() => SwitchWeapon(_currentIndex - 1);
    }
}

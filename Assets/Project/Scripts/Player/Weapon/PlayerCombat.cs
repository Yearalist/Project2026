using System;
using UnityEngine;

namespace ToySiege.Player.Combat
{
    public class PlayerCombat : MonoBehaviour
    {
        /// <summary> ADS durumu değiştiğinde (isAiming) </summary>
        public event Action<bool> OnAimChanged;

        [SerializeField] private Weapon _currentWeapon;
        [SerializeField] private WeaponSwitcher _weaponSwitcher;
        [SerializeField] private float _combatExitDelay = 2f;

        private static readonly int FireTrigger = Animator.StringToHash("Fire");
        private static readonly int IsCombat = Animator.StringToHash("isCombat");

        private Animator _baseAnimator;
        private float _lastFireTime = -999f;
        private bool _isInCombat;
        private bool _isAiming;

        public bool IsAiming => _isAiming;

        private void Awake()
        {
            _baseAnimator = GetComponentInChildren<Animator>();

            // WeaponSwitcher varsa ondan aktif silahı al
            if (_weaponSwitcher == null)
                _weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();

            if (_weaponSwitcher != null)
            {
                _currentWeapon = _weaponSwitcher.CurrentWeapon;
                _weaponSwitcher.OnWeaponChanged += OnWeaponChanged;
            }
        }

        private void OnDestroy()
        {
            if (_weaponSwitcher != null)
                _weaponSwitcher.OnWeaponChanged -= OnWeaponChanged;
        }

        private void OnWeaponChanged(Weapon newWeapon)
        {
            _currentWeapon = newWeapon;
        }

        private void Update()
        {
            if (_currentWeapon == null) return;

            bool firePressed = _currentWeapon.IsAutomatic
                ? Input.GetMouseButton(0)
                : Input.GetMouseButtonDown(0);

            if (firePressed)
                TryFire();

            if (Input.GetKeyDown(KeyCode.R))
                _currentWeapon.Reload();

            // ADS — sağ tık
            UpdateADS();
            UpdateCombatMode();
        }

        private void TryFire()
        {
            if (!_currentWeapon.CanFire()) return;

            _currentWeapon.Fire();
            _lastFireTime = Time.time;

            // Combat moduna gir (hen�z de�ilse)
            if (!_isInCombat)
            {
                _isInCombat = true;
                if (_baseAnimator != null)
                    _baseAnimator.SetBool(IsCombat, true);
            }

            // Fire animasyonunu tetikle
            if (_baseAnimator != null)
                _baseAnimator.SetTrigger(FireTrigger);
        }

        private void UpdateCombatMode()
        {
            // Son at��tan _combatExitDelay saniye ge�tiyse combat'tan ��k
            if (_isInCombat && Time.time - _lastFireTime > _combatExitDelay)
            {
                _isInCombat = false;
                if (_baseAnimator != null)
                    _baseAnimator.SetBool(IsCombat, false);
            }
        }

        private void UpdateADS()
        {
            bool wantsAim = Input.GetMouseButton(1); // sağ tık

            if (wantsAim != _isAiming)
            {
                _isAiming = wantsAim;
                OnAimChanged?.Invoke(_isAiming);

                // FOV geçişini GameFeelManager üzerinden yap (çakışma olmasın)
                if (GameFeelManager.Instance != null)
                    GameFeelManager.Instance.SetAiming(_isAiming);
            }
        }

        public void EquipWeapon(Weapon weapon) => _currentWeapon = weapon;
    }
}
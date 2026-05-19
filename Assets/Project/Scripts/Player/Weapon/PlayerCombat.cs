using UnityEngine;

namespace ToySiege.Player.Combat
{
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private Weapon _currentWeapon;
        [SerializeField] private float _combatExitDelay = 2f; // Son atýþtan 2sn sonra combat çýk

        private static readonly int FireTrigger = Animator.StringToHash("Fire");
        private static readonly int IsCombat = Animator.StringToHash("isCombat");

        private Animator _baseAnimator;
        private float _lastFireTime = -999f;
        private bool _isInCombat;

        private void Awake()
        {
            _baseAnimator = GetComponentInChildren<Animator>();
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

            // Combat modu otomatik çýkýþ
            UpdateCombatMode();
        }

        private void TryFire()
        {
            if (!_currentWeapon.CanFire()) return;

            _currentWeapon.Fire();
            _lastFireTime = Time.time;

            // Combat moduna gir (henüz deðilse)
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
            // Son atýþtan _combatExitDelay saniye geçtiyse combat'tan çýk
            if (_isInCombat && Time.time - _lastFireTime > _combatExitDelay)
            {
                _isInCombat = false;
                if (_baseAnimator != null)
                    _baseAnimator.SetBool(IsCombat, false);
            }
        }

        public void EquipWeapon(Weapon weapon) => _currentWeapon = weapon;
    }
}
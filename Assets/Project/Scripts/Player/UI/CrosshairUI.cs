using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ToySiege.Player.Combat;

namespace ToySiege.UI
{
    /// <summary>
    /// Dinamik crosshair sistemi.
    ///
    /// 4 adet Image (üst/alt/sol/sağ) kullanarak açılıp kapanan nişangah oluşturur.
    /// Ateş edildiğinde spread artar (açılır), zamanla geri döner.
    /// İsabet olduğunda kısa bir hit marker flash gösterir.
    ///
    /// KURULUM:
    ///   1) Canvas'ta boş bir GameObject oluştur, CrosshairUI ekle
    ///   2) 4 adet küçük dikdörtgen Image (crosshair line) oluştur, _lines dizisine ata
    ///   3) Ortada bir Image (dot) oluştur → _centerDot'a ata (opsiyonel)
    ///   4) Hit marker için 4 adet çapraz çizgi Image → _hitMarkerLines (opsiyonel)
    ///   5) Weapon referansını ata
    /// </summary>
    public class CrosshairUI : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private Weapon _weapon;
        [SerializeField] private PlayerCombat _playerCombat;
        [SerializeField] private RectTransform[] _lines = new RectTransform[4]; // üst, alt, sol, sağ
        [SerializeField] private Image _centerDot;

        [Header("Hit Marker")]
        [SerializeField] private CanvasGroup _hitMarkerGroup;
        [SerializeField] private Color _hitMarkerColor = Color.red;
        [SerializeField] private float _hitMarkerDuration = 0.15f;

        [Header("Spread Ayarları")]
        [Tooltip("Ateş etmezken crosshair çizgilerin merkeze uzaklığı")]
        [SerializeField] private float _baseSpread = 8f;
        [Tooltip("Ateş anında eklenen spread")]
        [SerializeField] private float _fireSpread = 20f;
        [Tooltip("Spread'in geri dönme hızı (saniye)")]
        [SerializeField] private float _recoverySpeed = 0.12f;
        [Tooltip("Ateş anında spread açılma hızı (saniye)")]
        [SerializeField] private float _snapSpeed = 0.04f;

        [Header("ADS (Nişan Alma)")]
        [Tooltip("ADS modunda crosshair spread'i")]
        [SerializeField] private float _adsSpread = 2f;
        [SerializeField] private Color _adsColor = new Color(1f, 0.3f, 0.3f, 1f);

        [Header("Renk")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _noAmmoColor = new Color(1f, 0.3f, 0.3f, 0.6f);

        private float _currentSpread;
        private float _targetSpread;
        private bool _isAiming;
        private Tweener _hitMarkerTween;

        // Her çizginin yön vektörü — pozisyon hesabında kullanılır
        // Sıra: üst(0,1), alt(0,-1), sol(-1,0), sağ(1,0)
        private static readonly Vector2[] LineDirections =
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };

        private void OnEnable()
        {
            if (_weapon != null)
            {
                _weapon.OnFired += HandleFired;
                _weapon.OnHit += HandleHit;
                _weapon.OnAmmoChanged += HandleAmmoChanged;
            }
            if (_playerCombat != null)
                _playerCombat.OnAimChanged += HandleAimChanged;
        }

        private void OnDisable()
        {
            if (_weapon != null)
            {
                _weapon.OnFired -= HandleFired;
                _weapon.OnHit -= HandleHit;
                _weapon.OnAmmoChanged -= HandleAmmoChanged;
            }
            if (_playerCombat != null)
                _playerCombat.OnAimChanged -= HandleAimChanged;
        }

        private void Start()
        {
            // PlayerCombat referansı atanmamışsa otomatik bul
            if (_playerCombat == null)
                _playerCombat = FindFirstObjectByType<PlayerCombat>();

            // Geç bağlama — Start'ta OnEnable'dan sonra çalışır
            if (_playerCombat != null)
                _playerCombat.OnAimChanged += HandleAimChanged;

            _currentSpread = _baseSpread;
            _targetSpread = _baseSpread;

            if (_hitMarkerGroup != null)
                _hitMarkerGroup.alpha = 0f;

            UpdateLinePositions();
            SetColor(_normalColor);
        }

        private void Update()
        {
            // ADS modunda hedef spread daha küçük
            float restSpread = _isAiming ? _adsSpread : _baseSpread;

            // Spread → rest'e doğru geri dön
            _targetSpread = Mathf.Lerp(_targetSpread, restSpread, Time.deltaTime / _recoverySpeed);
            _currentSpread = Mathf.Lerp(_currentSpread, _targetSpread, Time.deltaTime / _snapSpeed);

            UpdateLinePositions();
        }

        // ══════════════════════════════
        //  EVENT HANDLER'LAR
        // ══════════════════════════════

        private void HandleAimChanged(bool isAiming)
        {
            _isAiming = isAiming;
            SetColor(isAiming ? _adsColor : _normalColor);
        }

        private void HandleFired()
        {
            float restSpread = _isAiming ? _adsSpread : _baseSpread;
            _targetSpread = restSpread + _fireSpread * (_isAiming ? 0.3f : 1f);
        }

        private void HandleHit(float damage, Vector3 worldPos)
        {
            ShowHitMarker();
        }

        private void HandleAmmoChanged(int current, int max)
        {
            SetColor(current <= 0 ? _noAmmoColor : _normalColor);
        }

        // ══════════════════════════════
        //  GÖRSEL GÜNCELLEME
        // ══════════════════════════════

        private void UpdateLinePositions()
        {
            for (int i = 0; i < _lines.Length && i < LineDirections.Length; i++)
            {
                if (_lines[i] == null) continue;
                _lines[i].anchoredPosition = LineDirections[i] * _currentSpread;
            }
        }

        private void SetColor(Color color)
        {
            foreach (var line in _lines)
            {
                if (line == null) continue;
                var img = line.GetComponent<Image>();
                if (img != null) img.color = color;
            }

            if (_centerDot != null)
                _centerDot.color = color;
        }

        private void ShowHitMarker()
        {
            if (_hitMarkerGroup == null) return;

            _hitMarkerTween?.Kill();

            // Hit marker rengini ayarla
            var images = _hitMarkerGroup.GetComponentsInChildren<Image>();
            foreach (var img in images)
                img.color = _hitMarkerColor;

            // Flash: hızlı görün → yavaş sönsün
            _hitMarkerGroup.alpha = 1f;
            _hitMarkerTween = _hitMarkerGroup
                .DOFade(0f, _hitMarkerDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        private void OnDestroy()
        {
            _hitMarkerTween?.Kill();
        }
    }
}

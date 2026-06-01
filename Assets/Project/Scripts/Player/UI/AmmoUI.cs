using UnityEngine;
using TMPro;
using DG.Tweening;
using ToySiege.Player.Combat;

namespace ToySiege.UI
{
    /// <summary>
    /// Mermi sayacı UI.
    ///
    /// Weapon.OnAmmoChanged event'ini dinler, TextMeshPro ile "12 / 30" formatında gösterir.
    /// Düşük mermi uyarısı (renk değişimi + titreşim) ve reload flash efekti içerir.
    ///
    /// KURULUM:
    ///   1) Canvas'ta TextMeshPro - Text (UI) oluştur
    ///   2) Bu script'i ekle, _ammoText'e TMP referansını ata
    ///   3) Weapon referansını ata
    /// </summary>
    public class AmmoUI : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private Weapon _weapon;
        [SerializeField] private TextMeshProUGUI _ammoText;

        [Header("Renk Ayarları")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _lowAmmoColor = new Color(1f, 0.3f, 0.2f);
        [SerializeField] private Color _emptyColor = new Color(0.8f, 0f, 0f);
        [SerializeField] private Color _reloadFlashColor = new Color(0.3f, 1f, 0.5f);

        [Header("Eşikler")]
        [Tooltip("Maks merminin bu yüzdesi altında low ammo uyarısı")]
        [SerializeField, Range(0f, 1f)] private float _lowAmmoThreshold = 0.3f;

        [Header("Animasyon")]
        [SerializeField] private float _punchScale = 0.15f;
        [SerializeField] private float _punchDuration = 0.15f;

        private Tweener _punchTween;
        private Tweener _colorTween;
        private int _lastAmmo = -1;

        private void OnEnable()
        {
            if (_weapon != null)
                _weapon.OnAmmoChanged += UpdateAmmo;
        }

        private void OnDisable()
        {
            if (_weapon != null)
                _weapon.OnAmmoChanged -= UpdateAmmo;
        }

        private void Start()
        {
            if (_weapon != null)
                UpdateAmmo(_weapon.CurrentAmmo, _weapon.MaxAmmo);
        }

        private void UpdateAmmo(int current, int max)
        {
            if (_ammoText == null) return;

            // Metin güncelle
            _ammoText.text = $"{current} / {max}";

            // Renk belirle
            Color targetColor;
            if (current <= 0)
                targetColor = _emptyColor;
            else if ((float)current / max <= _lowAmmoThreshold)
                targetColor = _lowAmmoColor;
            else
                targetColor = _normalColor;

            _ammoText.color = targetColor;

            // Reload tespiti: mermi artıyorsa reload yapılmış
            if (_lastAmmo >= 0 && current > _lastAmmo)
            {
                PlayReloadFlash();
            }
            else if (current < _lastAmmo)
            {
                // Ateş edildi — küçük punch
                PlayFirePunch();
            }

            _lastAmmo = current;
        }

        private void PlayFirePunch()
        {
            _punchTween?.Kill();
            transform.localScale = Vector3.one;
            _punchTween = transform
                .DOPunchScale(Vector3.one * _punchScale, _punchDuration, vibrato: 4)
                .SetUpdate(true);
        }

        private void PlayReloadFlash()
        {
            if (_ammoText == null) return;

            _colorTween?.Kill();

            // Yeşil flash → normal renge dön
            _ammoText.color = _reloadFlashColor;
            _colorTween = DOTween.To(
                () => _ammoText.color,
                x => _ammoText.color = x,
                _normalColor,
                0.4f
            ).SetEase(Ease.OutQuad).SetUpdate(true);

            // Büyük punch
            _punchTween?.Kill();
            transform.localScale = Vector3.one;
            _punchTween = transform
                .DOPunchScale(Vector3.one * _punchScale * 1.5f, 0.25f, vibrato: 6)
                .SetUpdate(true);
        }

        private void OnDestroy()
        {
            _punchTween?.Kill();
            _colorTween?.Kill();
        }
    }
}

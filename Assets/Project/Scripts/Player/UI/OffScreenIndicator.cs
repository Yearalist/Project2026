using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToySiege.UI
{
    /// <summary>
    /// Ekran dışındaki düşmanları ekranın kenarında ok simgesiyle gösterir.
    ///
    /// Sahnedeki "Enemy" tag'li objeleri otomatik takip eder.
    /// Ekran içindeki düşmanlar için gösterge gizlenir.
    ///
    /// KURULUM:
    ///   1) Canvas'a boş GameObject ekle, OffScreenIndicator ata
    ///   2) Ok simgesi olan bir Image prefab oluştur → _indicatorPrefab
    ///   3) Canvas'taki RectTransform'u _canvasRect'e ata
    ///   4) Silah kullanılmıyorken göstergeleri gizlemek istersen _showOnlyInCombat = true
    /// </summary>
    public class OffScreenIndicator : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private GameObject _indicatorPrefab;
        [SerializeField] private RectTransform _canvasRect;
        [SerializeField] private Camera _cam;

        [Header("Ayarlar")]
        [Tooltip("Ekran kenarından içeri doğru padding (piksel)")]
        [SerializeField] private float _edgePadding = 50f;
        [Tooltip("Göstergelerin güncellenme aralığı (saniye) — performans için")]
        [SerializeField] private float _updateInterval = 0.1f;
        [Tooltip("Bu mesafenin ötesindeki düşmanlar gösterilmez")]
        [SerializeField] private float _maxTrackDistance = 80f;

        [Header("Renk")]
        [SerializeField] private Color _normalColor = new Color(1f, 0.4f, 0.2f, 0.8f);
        [SerializeField] private Color _closeColor = new Color(1f, 0.1f, 0.1f, 1f);
        [SerializeField] private float _closeDistance = 15f;

        private readonly Dictionary<Transform, RectTransform> _indicators = new();
        private readonly List<Transform> _toRemove = new();
        private float _updateTimer;

        private void Start()
        {
            if (_cam == null) _cam = Camera.main;
        }

        private void LateUpdate()
        {
            _updateTimer -= Time.deltaTime;
            if (_updateTimer > 0f) return;
            _updateTimer = _updateInterval;

            UpdateIndicators();
        }

        private void UpdateIndicators()
        {
            if (_cam == null) return;

            // Sahne'deki tüm düşmanları bul
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");

            // Var olan göstergeleri kontrol et — yok olmuş düşmanları temizle
            _toRemove.Clear();
            foreach (var kvp in _indicators)
            {
                if (kvp.Key == null)
                    _toRemove.Add(kvp.Key);
            }
            foreach (var key in _toRemove)
            {
                Destroy(_indicators[key].gameObject);
                _indicators.Remove(key);
            }

            // Her düşman için gösterge güncelle
            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                Transform enemyT = enemy.transform;
                float dist = Vector3.Distance(_cam.transform.position, enemyT.position);

                // Çok uzaktaysa atla
                if (dist > _maxTrackDistance)
                {
                    if (_indicators.ContainsKey(enemyT))
                    {
                        _indicators[enemyT].gameObject.SetActive(false);
                    }
                    continue;
                }

                // Viewport pozisyonu
                Vector3 viewPos = _cam.WorldToViewportPoint(enemyT.position);

                // Kameranın arkasındaysa → viewport'u ters çevir
                bool isBehind = viewPos.z < 0;
                if (isBehind)
                {
                    viewPos.x = 1f - viewPos.x;
                    viewPos.y = 1f - viewPos.y;
                }

                bool isOnScreen = !isBehind
                    && viewPos.x > 0.05f && viewPos.x < 0.95f
                    && viewPos.y > 0.05f && viewPos.y < 0.95f;

                // Gösterge al veya oluştur
                if (!_indicators.TryGetValue(enemyT, out RectTransform indicator))
                {
                    if (isOnScreen) continue; // Ekrandaysa gösterge oluşturma

                    var obj = Instantiate(_indicatorPrefab, transform);
                    indicator = obj.GetComponent<RectTransform>();
                    _indicators[enemyT] = indicator;
                }

                if (isOnScreen)
                {
                    indicator.gameObject.SetActive(false);
                    continue;
                }

                indicator.gameObject.SetActive(true);

                // Ekran kenarında pozisyon hesapla
                Vector2 canvasSize = _canvasRect.sizeDelta;
                Vector2 screenCenter = canvasSize * 0.5f;

                // Viewport → canvas koordinatı
                Vector2 dir = new Vector2(
                    viewPos.x - 0.5f,
                    viewPos.y - 0.5f
                ).normalized;

                // Kenar sınırları
                float halfW = (canvasSize.x * 0.5f) - _edgePadding;
                float halfH = (canvasSize.y * 0.5f) - _edgePadding;

                // Yön vektörünü kenaralara clamp et
                float scaleX = dir.x != 0 ? Mathf.Abs(halfW / dir.x) : float.MaxValue;
                float scaleY = dir.y != 0 ? Mathf.Abs(halfH / dir.y) : float.MaxValue;
                float scale = Mathf.Min(scaleX, scaleY);

                Vector2 pos = screenCenter + dir * scale;
                indicator.anchoredPosition = pos;

                // Oku düşmana doğru döndür
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                indicator.localRotation = Quaternion.Euler(0, 0, angle - 90f);

                // Yakınlığa göre renk
                var img = indicator.GetComponent<Image>();
                if (img != null)
                    img.color = dist < _closeDistance ? _closeColor : _normalColor;
            }
        }

        private void OnDestroy()
        {
            foreach (var kvp in _indicators)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            _indicators.Clear();
        }
    }
}

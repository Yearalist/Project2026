using UnityEngine;

namespace ToySiege.UI
{
    /// <summary>
    /// Minimap kamerası — oyuncuyu üstten takip eder.
    ///
    /// Ayrı bir Camera component'i ile RenderTexture'a çizer.
    /// UI'daki Raw Image bu RenderTexture'ı gösterir.
    ///
    /// KURULUM:
    ///   1) Sahneye yeni Camera oluştur: MinimapCamera
    ///   2) Rotation: (90, 0, 0) — aşağı baksın
    ///   3) Projection: Orthographic, Size: 30-50
    ///   4) Clear Flags: Solid Color (koyu renk)
    ///   5) Culling Mask: sadece görmek istediğin layer'lar
    ///   6) Project'te sağ tık → Create → Render Texture: "MinimapRT" (256×256 veya 512×512)
    ///   7) Kameranın Target Texture → MinimapRT
    ///   8) Canvas'ta Raw Image → Texture = MinimapRT
    ///   9) Bu script'i kameraya ekle, _target → oyuncu Transform
    /// </summary>
    public class MinimapCamera : MonoBehaviour
    {
        [Header("Takip")]
        [SerializeField] private Transform _target;
        [SerializeField] private float _height = 40f;

        [Header("Zoom")]
        [SerializeField] private float _minZoom = 20f;
        [SerializeField] private float _maxZoom = 60f;
        [SerializeField] private float _zoomSpeed = 10f;

        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();

            // Minimap kamerasında AudioListener olmamalı
            var listener = GetComponent<AudioListener>();
            if (listener != null)
                Destroy(listener);

            if (_target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    _target = player.transform;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Oyuncuyu takip et (sadece XZ, sabit Y)
            Vector3 pos = _target.position;
            pos.y = _target.position.y + _height;
            transform.position = pos;

            // Oyuncunun yönüne göre minimap'i döndür (kuzey = oyuncunun baktığı yön)
            transform.rotation = Quaternion.Euler(90f, _target.eulerAngles.y, 0f);
        }

        /// <summary>
        /// Minimap zoom'unu ayarla. UI slider'dan çağrılabilir.
        /// </summary>
        public void SetZoom(float value)
        {
            if (_cam == null) return;
            _cam.orthographicSize = Mathf.Lerp(_minZoom, _maxZoom, value);
        }
    }
}

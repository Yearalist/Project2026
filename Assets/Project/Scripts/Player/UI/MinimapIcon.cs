using UnityEngine;

namespace ToySiege.UI
{
    /// <summary>
    /// Minimap'te görünmesi gereken objelere eklenir.
    /// Objenin üzerinde küçük bir quad oluşturarak
    /// minimap kamerasının göreceği bir ikon yerleştirir.
    ///
    /// Prefab atanmazsa otomatik olarak renkli bir quad oluşturur.
    ///
    /// KURULUM:
    ///   1) Düşman/oyuncu/önemli objelere bu script'i ekle
    ///   2) Renk ayarla (mavi = oyuncu, kırmızı = düşman)
    ///   3) "Minimap" layer oluştur → minimap kamerasının Culling Mask'ına ekle
    ///      ana kameranın mask'ından çıkar
    ///   4) (Opsiyonel) _iconPrefab ile özel ikon kullanabilirsin
    /// </summary>
    public class MinimapIcon : MonoBehaviour
    {
        [Header("İkon Ayarları")]
        [SerializeField] private GameObject _iconPrefab;
        [SerializeField] private float _iconHeight = 10f;
        [SerializeField] private float _iconScale = 5f;
        [SerializeField] private bool _rotateWithParent = true;

        [Header("Otomatik İkon (Prefab yoksa)")]
        [SerializeField] private Color _iconColor = Color.red;

        [Header("Layer")]
        [Tooltip("Minimap layer adı — otomatik atanır")]
        [SerializeField] private string _minimapLayerName = "Minimap";

        private GameObject _iconInstance;

        private void Start()
        {
            if (_iconPrefab != null)
            {
                _iconInstance = Instantiate(_iconPrefab);
            }
            else
            {
                // Prefab yoksa otomatik quad oluştur
                _iconInstance = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _iconInstance.name = $"MinimapIcon_{gameObject.name}";

                // Collider'ı kaldır (fizik etkileşimi olmasın)
                var col = _iconInstance.GetComponent<Collider>();
                if (col != null) Destroy(col);

                // Renk ayarla
                var renderer = _iconInstance.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Shader bul — URP veya built-in fallback
                    Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                    if (shader == null) shader = Shader.Find("Unlit/Color");
                    if (shader == null) shader = Shader.Find("UI/Default");
                    if (shader == null) shader = Shader.Find("Sprites/Default");

                    Material mat;
                    if (shader != null)
                    {
                        mat = new Material(shader);
                        mat.color = _iconColor;
                    }
                    else
                    {
                        // Hiçbir shader bulunamazsa mevcut material'ı kullan
                        mat = new Material(renderer.material);
                        mat.color = _iconColor;
                        Debug.LogWarning("[MinimapIcon] Uygun shader bulunamadı, varsayılan material kullanılıyor.");
                    }

                    renderer.material = mat;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
            }

            // Pozisyon ve scale ayarla
            _iconInstance.transform.position = transform.position + Vector3.up * _iconHeight;
            _iconInstance.transform.localScale = Vector3.one * _iconScale;

            // Yüzü yukarı baksın (minimap kamerasına dönük)
            _iconInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Layer ata
            int layer = LayerMask.NameToLayer(_minimapLayerName);
            if (layer != -1)
            {
                SetLayerRecursive(_iconInstance, layer);
                Debug.Log($"<color=cyan>[MinimapIcon] {gameObject.name} ikonu oluşturuldu! Layer: {_minimapLayerName} ({layer}), Pos: {_iconInstance.transform.position}</color>");
            }
            else
            {
                // Layer yoksa Default layer'da bırak — yine de görünür olsun
                Debug.LogWarning($"<color=red>[MinimapIcon] '{_minimapLayerName}' layer bulunamadı!</color> Edit → Project Settings → Tags and Layers'dan ekle. İkon Default layer'da oluşturuldu.");
            }
        }

        private void LateUpdate()
        {
            if (_iconInstance == null) return;

            _iconInstance.transform.position = transform.position + Vector3.up * _iconHeight;

            if (_rotateWithParent)
            {
                // Parent'ın Y rotasyonunu al, X=90 (yukarı baksın)
                _iconInstance.transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
            }
            else
            {
                _iconInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }

        private static void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursive(child.gameObject, layer);
        }

        private void OnDestroy()
        {
            if (_iconInstance != null)
                Destroy(_iconInstance);
        }
    }
}

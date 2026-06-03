using UnityEngine;

namespace ToySiege.Core
{
    /// <summary>
    /// VFX prefab'larına ekle — belirli süre sonra otomatik yok olur.
    /// ParticleSystem varsa bitene kadar bekler, yoksa sabit süre sonra yok eder.
    ///
    /// KURULUM:
    ///   VFX prefab'ına bu script'i ekle. Başka bir şey yapma.
    /// </summary>
    public class AutoDestroyVFX : MonoBehaviour
    {
        [Tooltip("ParticleSystem yoksa bu süre sonra yok edilir")]
        [SerializeField] private float _fallbackLifetime = 2f;

        private ParticleSystem _ps;

        private void Start()
        {
            _ps = GetComponent<ParticleSystem>();

            if (_ps != null)
            {
                // Partikül sisteminin toplam süresini al
                var main = _ps.main;
                float totalDuration = main.duration + main.startLifetime.constantMax;
                Destroy(gameObject, totalDuration + 0.5f);
            }
            else
            {
                Destroy(gameObject, _fallbackLifetime);
            }
        }
    }
}

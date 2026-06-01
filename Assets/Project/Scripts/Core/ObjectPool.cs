using System.Collections.Generic;
using UnityEngine;

namespace ToySiege.Core
{
    /// <summary>
    /// Genel amaçlı Object Pool.
    ///
    /// Instantiate/Destroy yerine bu sistemi kullanarak GC spike'larını önler.
    /// Mermi trail, impact efekt, ses kaynakları gibi kısa ömürlü objeler için ideal.
    ///
    /// KULLANIM:
    ///   var pool = new ObjectPool(prefab, parent, initialSize: 10);
    ///   GameObject obj = pool.Get();          // havuzdan al (veya yeni oluştur)
    ///   pool.Return(obj);                     // havuza geri koy
    ///
    /// NOT: Return çağrılmazsa obje yine çalışır ama pool avantajı kaybolur.
    ///      AutoReturn component'i ile otomatik geri dönüş sağlanabilir.
    /// </summary>
    public class ObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Queue<GameObject> _available = new Queue<GameObject>();

        public ObjectPool(GameObject prefab, Transform parent = null, int initialSize = 0)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                var obj = CreateNew();
                obj.SetActive(false);
                _available.Enqueue(obj);
            }
        }

        /// <summary>
        /// Havuzdan bir obje al. Havuz boşsa yeni oluşturur.
        /// Obje aktif halde döner.
        /// </summary>
        public GameObject Get()
        {
            GameObject obj;

            while (_available.Count > 0)
            {
                obj = _available.Dequeue();
                if (obj != null)
                {
                    obj.SetActive(true);
                    return obj;
                }
                // Destroy edilmişse atla
            }

            obj = CreateNew();
            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Havuzdan bir obje al ve belirtilen pozisyon/rotasyona yerleştir.
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            var obj = Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        /// <summary>
        /// Objeyi havuza geri koy. Deaktif edilir.
        /// </summary>
        public void Return(GameObject obj)
        {
            if (obj == null) return;
            obj.SetActive(false);
            _available.Enqueue(obj);
        }

        private GameObject CreateNew()
        {
            return Object.Instantiate(_prefab, _parent);
        }
    }
}

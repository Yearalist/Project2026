using UnityEngine;

namespace ToySiege.Player.Health
{
    public class PlayerRagdoll : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private Animator _animator;

        private Rigidbody[] _ragdollBodies;
        private Collider[] _ragdollColliders;
        private CharacterController _cc;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();

            // Model altýndaki tüm rigidbody ve collider'larý bul
            _ragdollBodies = _animator.GetComponentsInChildren<Rigidbody>();
            _ragdollColliders = _animator.GetComponentsInChildren<Collider>();

            // Baþlangýçta ragdoll kapalý
            SetRagdollActive(false);
        }

        private void OnEnable()
        {
            if (_playerHealth != null)
                _playerHealth.OnDeath += ActivateRagdoll;
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
                _playerHealth.OnDeath -= ActivateRagdoll;
        }

        private void ActivateRagdoll()
        {
            // Animator'ý kapat — fizik devralacak
            _animator.enabled = false;

            // CharacterController'ý kapat — ragdoll collider'larý çakýþmasýn
            if (_cc != null)
                _cc.enabled = false;

            // Player hareketini durdur
            var controller = GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;

            // Ragdoll aç
            SetRagdollActive(true);

            // Son hareket yönünde hafif itme — doðal düþüþ
            if (_ragdollBodies.Length > 0)
            {
                Vector3 force = -transform.forward * 2f + Vector3.up * 3f;
                _ragdollBodies[0].AddForce(force, ForceMode.VelocityChange);
            }
        }

        private void SetRagdollActive(bool active)
        {
            foreach (var rb in _ragdollBodies)
            {
                // Player'ýn kendi Rigidbody'si varsa atla
                if (rb.gameObject == gameObject) continue;

                rb.isKinematic = !active;
                rb.useGravity = active;
            }

            foreach (var col in _ragdollColliders)
            {
                if (col.gameObject == gameObject) continue;
                col.enabled = active;
            }
        }
    }
}
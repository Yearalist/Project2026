using ToySiege.Enemy.Data;
using UnityEngine;

namespace ToySiege.Enemy
{
    public class EnemyDetection : MonoBehaviour
    {
        private EnemyConfig _config;
        private Transform _target;

        // ★ ALARM SİSTEMİ
        private bool _isAlerted = false;
        public bool IsAlerted => _isAlerted;

        public Transform Target => _target;
        public bool HasTarget => _target != null;
        public float DistanceToTarget { get; private set; }
        public bool IsInAttackRange => HasTarget && DistanceToTarget <= _config.AttackRange;
        public bool IsInDetectionRange => HasTarget && DistanceToTarget <= _config.DetectionRange;
        public bool IsInLoseRange => HasTarget && DistanceToTarget <= _config.LoseRange;

        public void Initialize(EnemyConfig config)
        {
            _config = config;
        }

        private void Update()
        {
            if (_target == null)
                TryFindPlayer();

            if (_target != null)
                DistanceToTarget = Vector3.Distance(transform.position, _target.position);
        }

        private void TryFindPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _target = player.transform;
        }

        public bool CanSeeTarget()
        {
            if (!HasTarget) return false;

            // ★ Alarm aldıysa FOV ve mesafe kontrolünü bypass et
            if (_isAlerted) return true;

            if (DistanceToTarget > _config.DetectionRange) return false;

            Vector3 dirToTarget = (_target.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            if (angle > _config.FieldOfView / 2f) return false;

            // Raycast ile görüş kontrolü
            if (Physics.Raycast(transform.position + Vector3.up, dirToTarget, out RaycastHit hit, _config.DetectionRange))
            {
                if (hit.transform == _target)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Polis arabası veya başka bir düşman tarafından çağrılır.
        /// FOV/mesafe kontrolünü bypass ederek doğrudan hedefe kilitler.
        /// Asker mevcut FSM state'i ne olursa olsun CanSeeTarget() true dönecek
        /// ve FSM otomatik olarak Chase/Attack state'ine geçecek.
        /// </summary>
        public void ForceAlert(Transform alertTarget)
        {
            if (alertTarget == null) return;

            _target = alertTarget;
            _isAlerted = true;
            DistanceToTarget = Vector3.Distance(transform.position, _target.position);

            Debug.Log($"<color=cyan>[{gameObject.name}] ALARM ALDI! Hedefe kilitlendi.</color>");
        }

        /// <summary>
        /// Alarm durumunu sıfırlar.
        /// Oyuncu LoseRange dışına çıktığında çağırabilirsin.
        /// </summary>
        public void ClearAlert()
        {
            _isAlerted = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (_config == null) return;

            // Detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _config.DetectionRange);

            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _config.AttackRange);

            // FOV
            Gizmos.color = Color.cyan;
            Vector3 leftBound = Quaternion.Euler(0, -_config.FieldOfView / 2f, 0) * transform.forward;
            Vector3 rightBound = Quaternion.Euler(0, _config.FieldOfView / 2f, 0) * transform.forward;
            Gizmos.DrawRay(transform.position + Vector3.up, leftBound * _config.DetectionRange);
            Gizmos.DrawRay(transform.position + Vector3.up, rightBound * _config.DetectionRange);

            // ★ Alarm durumunda kırmızı gösterge
            if (_isAlerted)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
            }
        }
    }
}
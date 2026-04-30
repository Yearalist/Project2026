using ToySiege.Enemy.Data;
using UnityEngine;

namespace ToySiege.Enemy
{
    public class EnemyDetection : MonoBehaviour
    {
        private EnemyConfig _config;
        private Transform _target;

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
            if (DistanceToTarget > _config.DetectionRange) return false;

            Vector3 dirToTarget = (_target.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle > _config.FieldOfView / 2f) return false;

            // Raycast ile görüþ kontrolü
            if (Physics.Raycast(transform.position + Vector3.up, dirToTarget, out RaycastHit hit, _config.DetectionRange))
            {
                if (hit.transform == _target)
                    return true;
            }

            return false;
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
        }
    }
}
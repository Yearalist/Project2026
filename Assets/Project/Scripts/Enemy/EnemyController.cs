using UnityEngine;
using UnityEngine.AI;
using ToySiege.Core.FSM;
using ToySiege.Enemy.Data;
using ToySiege.Enemy.States;

namespace ToySiege.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyDetection))]
    [RequireComponent(typeof(EnemyAnimator))]
    public class EnemyController : MonoBehaviour, ToySiege.Combat.IDamageable
    {
        [Header("Konfigürasyon")]
        [SerializeField] private EnemyConfig _config;

        public EnemyConfig Config => _config;
        public NavMeshAgent Agent { get; private set; }
        public EnemyDetection Detection { get; private set; }
        public EnemyAnimator Anim { get; private set; }
        public StateMachine FSM { get; private set; }

        // Sağlık
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        // Attack
        public float AttackCooldownTimer { get; set; }
        public bool CanAttack => AttackCooldownTimer <= 0f;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Detection = GetComponent<EnemyDetection>();
            Anim = GetComponent<EnemyAnimator>();

            Detection.Initialize(_config);

            Agent.speed = _config.MoveSpeed;
            Agent.stoppingDistance = _config.StoppingDistance;
            Agent.angularSpeed = _config.RotationSpeed;

            CurrentHealth = _config.MaxHealth;

            FSM = new StateMachine();
            var factory = new EnemyStateFactory(this);
            FSM.Initialize(factory.Idle());

            FSM.OnStateChanged += (prev, next) =>
            {
                Debug.Log($"<color=red>[Enemy FSM] {prev?.GetType().Name ?? "None"} → {next.GetType().Name}</color>");
            };
        }

        private void Update()
        {
            if (IsDead) return;

            FSM.Update();

            if (AttackCooldownTimer > 0f)
                AttackCooldownTimer -= Time.deltaTime;

            // NavMeshAgent hızını Animator'a gönder
            Anim.UpdateSpeed(Agent.velocity.magnitude);
        }

        private void FixedUpdate()
        {
            if (IsDead) return;
            FSM.FixedUpdate();
        }

        // ── Hareket ──
        public void ChaseTarget()
        {
            if (Detection.HasTarget)
            {
                Agent.speed = _config.ChaseSpeed;
                Agent.SetDestination(Detection.Target.position);
            }
        }

        public void StopMoving()
        {
            Agent.ResetPath();
            Agent.velocity = Vector3.zero;
        }

        public void LookAtTarget()
        {
            if (!Detection.HasTarget) return;

            Vector3 dir = (Detection.Target.position - transform.position).normalized;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot,
                    _config.RotationSpeed * Time.deltaTime
                );
            }
        }

        // ── Hasar ──
        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (IsDead) return;

            CurrentHealth -= damage;
            Debug.Log($"<color=orange>[Enemy] Hasar: {damage} | HP: {CurrentHealth}</color>");

            if (IsDead)
            {
                Anim.TriggerDie();
                Agent.enabled = false;
                // Hit kuvveti uygula — ileride ragdoll bağlandığında ilk impulse
                Destroy(gameObject, 3f);
            }
            else
            {
                Anim.TriggerHit();
            }
        }

        // ── Patrol ──
        public Vector3 GetRandomPatrolPoint()
        {
            Vector3 randomDir = Random.insideUnitSphere * _config.PatrolRadius;
            randomDir += transform.position;

            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, _config.PatrolRadius, NavMesh.AllAreas))
                return hit.position;

            return transform.position;
        }
    }
}
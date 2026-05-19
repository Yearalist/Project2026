using ToySiege.Player.Health;
using UnityEngine;
using UnityEngine.AI;

namespace ToySiege.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PoliceCarEnemy : MonoBehaviour, ToySiege.Combat.IDamageable
    {
        [Header("=== Referanslar ===")]
        public WheelSpinner[] wheels;

        [Header("=== Devriye Ayarları ===")]
        public Transform[] patrolPoints;
        public float patrolSpeed = 5f;
        public float waypointThreshold = 2f;
        public float turnPauseTime = 0.3f;

        [Header("=== Algılama & Takip ===")]
        public float detectionRange = 25f;
        [Tooltip("Takipte oyuncuyu kaybetme mesafesi")]
        public float loseRange = 35f;
        public float chaseSpeed = 10f;
        [Tooltip("Bu mesafede dash tetiklenir")]
        public float dashTriggerRange = 12f;
        public float dashWindupTime = 0.8f;

        [Header("=== Dash Saldırı ===")]
        public float dashSpeed = 20f;
        public float dashStopDistance = 2f;
        public float dashMaxTime = 3f;
        public float dashCooldown = 3f;

        [Header("=== Hasar ===")]
        public float dashDamage = 40f;
        public float normalHitDamage = 15f;
        public float knockbackForce = 20f;
        public float knockUpForce = 8f;

        [Header("=== Alarm Sistemi ===")]
        [Tooltip("Çarptıktan sonra bu yarıçaptaki düşmanları uyarır")]
        public float alertRadius = 40f;

        [Header("=== Sağlık ===")]
        public float maxHealth = 200f;
        private float _currentHealth;

        [Header("=== Görsel Efektler ===")]
        public ParticleSystem dashParticle;
        public GameObject sirenLight;
        public AudioClip dashHornSound;
        public AudioClip hitSound;
        public AudioClip alertSound;

        // Durum
        public enum State { Patrol, TurnPause, Chase, WindUp, Dash, Cooldown, Dead }
        [HideInInspector] public State currentState = State.Patrol;

        // ── IDamageable implementasyonu ──
        public bool IsDead => _currentHealth <= 0f;

        // Private
        private NavMeshAgent _agent;
        private AudioSource _audio;
        private Transform _player;
        private int _patrolIndex = 0;
        private float _stateTimer = 0f;
        private Vector3 _dashTarget;
        private bool _hasHitThisDash = false;
        private bool _isAlerted = false;

        // ══════════════════════════════════════
        //  LIFECYCLE
        // ══════════════════════════════════════

        void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _audio = GetComponent<AudioSource>();
            if (_audio == null)
                _audio = gameObject.AddComponent<AudioSource>();

            _currentHealth = maxHealth;

            // Oyuncuyu bul
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;

            // NavMeshAgent - keskin dönüş ayarları
            _agent.speed = patrolSpeed;
            _agent.angularSpeed = 0f;
            _agent.acceleration = 100f;
            _agent.autoBraking = true;
            _agent.updateRotation = false;

            SetSiren(false);
        }

        void Update()
        {
            if (currentState == State.Dead) return;
            if (_player == null) return;

            float dist = Vector3.Distance(transform.position, _player.position);
            UpdateWheels();

            switch (currentState)
            {
                case State.Patrol:
                    DoPatrol();
                    if (dist < detectionRange || _isAlerted)
                        ChangeState(State.Chase);
                    break;

                case State.TurnPause:
                    DoTurnPause();
                    if (dist < detectionRange || _isAlerted)
                        ChangeState(State.Chase);
                    break;

                case State.Chase:
                    DoChase();
                    if (dist > loseRange && !_isAlerted)
                        ChangeState(State.Patrol);
                    else if (dist < dashTriggerRange)
                        ChangeState(State.WindUp);
                    break;

                case State.WindUp:
                    DoWindUp();
                    break;

                case State.Dash:
                    DoDash();
                    break;

                case State.Cooldown:
                    DoCooldown();
                    break;
            }
        }

        // ══════════════════════════════════════
        //  DURUMLAR
        // ══════════════════════════════════════

        void DoPatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                _agent.isStopped = true;
                return;
            }

            _agent.isStopped = false;
            _agent.speed = patrolSpeed;
            _agent.SetDestination(patrolPoints[_patrolIndex].position);

            FaceMovementDirection();

            if (_agent.remainingDistance < waypointThreshold && !_agent.pathPending)
            {
                _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
                ChangeState(State.TurnPause);
            }
        }

        void DoTurnPause()
        {
            _agent.isStopped = true;
            _stateTimer -= Time.deltaTime;

            if (_stateTimer <= 0f)
            {
                if (patrolPoints.Length > 0)
                {
                    Vector3 dir = (patrolPoints[_patrolIndex].position - transform.position).normalized;
                    dir.y = 0;
                    if (dir.sqrMagnitude > 0.01f)
                        transform.rotation = Quaternion.LookRotation(dir);
                }
                ChangeState(State.Patrol);
            }
        }

        void DoChase()
        {
            _agent.isStopped = false;
            _agent.speed = chaseSpeed;
            _agent.SetDestination(_player.position);
            SetSiren(true);
            FaceMovementDirection();
        }

        void DoWindUp()
        {
            _agent.isStopped = true;

            Vector3 lookDir = (_player.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(lookDir);

            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0f)
                ChangeState(State.Dash);
        }

        void DoDash()
        {
            _agent.isStopped = true;

            Vector3 dirToTarget = (_dashTarget - transform.position);
            dirToTarget.y = 0;

            if (dirToTarget.magnitude < dashStopDistance || _stateTimer <= 0f)
            {
                ChangeState(State.Cooldown);
                return;
            }

            Vector3 moveDir = dirToTarget.normalized;
            transform.position += moveDir * dashSpeed * Time.deltaTime;

            if (moveDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(moveDir);

            _stateTimer -= Time.deltaTime;
        }

        void DoCooldown()
        {
            _agent.isStopped = true;
            _stateTimer -= Time.deltaTime;

            if (_stateTimer <= 0f)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                    _agent.Warp(hit.position);

                _isAlerted = false;
                ChangeState(State.Chase);
            }
        }

        // ══════════════════════════════════════
        //  DURUM GEÇİŞ
        // ══════════════════════════════════════

        void ChangeState(State newState)
        {
            if (currentState == newState) return;

            currentState = newState;

            switch (newState)
            {
                case State.Patrol:
                    SetSiren(false);
                    StopDashParticle();
                    break;

                case State.TurnPause:
                    _stateTimer = turnPauseTime;
                    break;

                case State.Chase:
                    SetSiren(true);
                    break;

                case State.WindUp:
                    _stateTimer = dashWindupTime;
                    if (dashHornSound != null)
                        _audio.PlayOneShot(dashHornSound);
                    break;

                case State.Dash:
                    _stateTimer = dashMaxTime;
                    _hasHitThisDash = false;
                    _dashTarget = _player.position;
                    if (dashParticle != null) dashParticle.Play();
                    break;

                case State.Cooldown:
                    _stateTimer = dashCooldown;
                    SetSiren(false);
                    StopDashParticle();
                    break;

                case State.Dead:
                    _agent.enabled = false;
                    SetSiren(false);
                    StopDashParticle();
                    break;
            }

            Debug.Log($"<color=blue>[Car FSM] → {newState}</color>");
        }

        // ══════════════════════════════════════
        //  IDamageable — HASAR ALMA
        // ══════════════════════════════════════

        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (IsDead) return;

            _currentHealth -= damage;
            Debug.Log($"<color=orange>[PoliceCar] Hasar: {damage} | HP: {_currentHealth}/{maxHealth}</color>");

            if (IsDead)
            {
                ChangeState(State.Dead);

                // Patlama efekti eklenebilir
                Debug.Log("<color=red>[PoliceCar] YIKILDI!</color>");
                Destroy(gameObject, 3f);
            }
            else
            {
                // Hasar aldığında alarm ver — etraftaki düşmanları uyar
                _isAlerted = true;
                if (currentState == State.Patrol || currentState == State.TurnPause)
                    ChangeState(State.Chase);
            }
        }

        // ══════════════════════════════════════
        //  ÇARPIŞMA & OYUNCUYA HASAR
        // ══════════════════════════════════════

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            float damage;
            float force;

            if (currentState == State.Dash && !_hasHitThisDash)
            {
                damage = dashDamage;
                force = knockbackForce;
                _hasHitThisDash = true;
                AlertNearbyEnemies();
            }
            else
            {
                damage = normalHitDamage;
                force = knockbackForce * 0.4f;
            }

            var health = other.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);

            var rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 knockDir = (other.transform.position - transform.position).normalized;
                knockDir.y = 0;
                rb.AddForce(knockDir * force + Vector3.up * knockUpForce, ForceMode.Impulse);
            }

            var cc = other.GetComponent<CharacterController>();
            if (cc != null && rb == null)
            {
                Debug.Log($"<color=red>[Car] Oyuncuya çarptı! Hasar: {damage}</color>");
            }

            if (hitSound != null)
                _audio.PlayOneShot(hitSound);
        }

        // ══════════════════════════════════════
        //  ALARM SİSTEMİ
        // ══════════════════════════════════════

        void AlertNearbyEnemies()
        {
            if (alertSound != null)
                _audio.PlayOneShot(alertSound);

            Collider[] nearby = Physics.OverlapSphere(transform.position, alertRadius);
            int count = 0;

            foreach (var col in nearby)
            {
                if (col.gameObject == gameObject) continue;
                if (!col.CompareTag("Enemy")) continue;

                var otherCar = col.GetComponent<PoliceCarEnemy>();
                if (otherCar != null && otherCar != this)
                {
                    otherCar.ReceiveAlert(_player);
                    count++;
                    continue;
                }

                var detection = col.GetComponent<EnemyDetection>();
                if (detection != null)
                {
                    detection.ForceAlert(_player);
                    count++;
                    continue;
                }

                var agent = col.GetComponent<NavMeshAgent>();
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.SetDestination(_player.position);
                    count++;
                }
            }

            Debug.Log($"<color=cyan>[ALARM] {count} düşman uyarıldı! (Yarıçap: {alertRadius}m)</color>");
        }

        public void ReceiveAlert(Transform target)
        {
            if (currentState == State.Dash || currentState == State.WindUp || currentState == State.Dead)
                return;

            _isAlerted = true;
            _player = target;
            ChangeState(State.Chase);
            Debug.Log($"<color=cyan>[{gameObject.name}] Alarm aldı!</color>");
        }

        // ══════════════════════════════════════
        //  YARDIMCI
        // ══════════════════════════════════════

        void FaceMovementDirection()
        {
            Vector3 vel = _agent.velocity;
            vel.y = 0;
            if (vel.magnitude < 0.5f) return;
            transform.rotation = Quaternion.LookRotation(vel.normalized);
        }

        void UpdateWheels()
        {
            bool moving = (_agent.velocity.magnitude > 0.5f && !_agent.isStopped)
                          || currentState == State.Dash;
            float speed = currentState == State.Dash ? dashSpeed : _agent.velocity.magnitude;

            foreach (var w in wheels)
            {
                if (w != null)
                {
                    w.SetMoving(moving);
                    w.SetSpeed(speed);
                }
            }
        }

        void SetSiren(bool on)
        {
            if (sirenLight != null) sirenLight.SetActive(on);
        }

        void StopDashParticle()
        {
            if (dashParticle != null) dashParticle.Stop();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, dashTriggerRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, alertRadius);

            if (patrolPoints != null && patrolPoints.Length > 1)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    if (patrolPoints[i] == null) continue;
                    int next = (i + 1) % patrolPoints.Length;
                    if (patrolPoints[next] == null) continue;
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
                }
            }
        }
    }
}
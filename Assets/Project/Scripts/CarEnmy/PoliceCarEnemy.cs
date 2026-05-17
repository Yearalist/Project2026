using ToySiege.Player.Health;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Polis arabasý düþman AI - Dash/Hýzlanma saldýrý mekaniði
/// 
/// KURULUM:
/// 1) Araba prefab'ýna NavMeshAgent ekle
/// 2) Bu scripti araba root objesine ekle
/// 3) Araba objesine "Enemy" tag'i ver
/// 4) Araba objesine Rigidbody (Is Kinematic = true) ve BoxCollider (Is Trigger = true) ekle
/// 5) Inspector'dan wheels dizisine 4 tekerlek objesini ata
/// 6) Player objesine "Player" tag'i ver
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class PoliceCarEnemy : MonoBehaviour
{
    [Header("=== Referanslar ===")]
    [Tooltip("Oyuncu Transform'u (boþ býrakýrsan 'Player' tag ile bulur)")]
    public Transform player;
    public WheelSpinner[] wheels;

    [Header("=== Devriye Ayarlarý ===")]
    [Tooltip("Devriye noktalarý (boþ býrakýrsan yerinde bekler)")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 5f;
    public float waypointThreshold = 2f;

    [Header("=== Algýlama ===")]
    public float detectionRange = 25f;
    [Tooltip("Dash baþlamadan önce oyuncuya bu mesafede kilitlenir")]
    public float dashTriggerRange = 15f;
    [Tooltip("Dash öncesi bekleme (korna/siren uyarýsý)")]
    public float dashWindupTime = 0.8f;

    [Header("=== Dash Saldýrý ===")]
    public float dashSpeed = 30f;
    public float dashDuration = 2f;
    [Tooltip("Dash sonrasý bekleme süresi (cooldown)")]
    public float dashCooldown = 3f;

    [Header("=== Hasar ===")]
    public float dashDamage = 40f;
    public float normalHitDamage = 15f;
    [Tooltip("Çarpma kuvveti (fýrlatma)")]
    public float knockbackForce = 20f;
    [Tooltip("Yukarý fýrlatma kuvveti")]
    public float knockUpForce = 8f;

    [Header("=== Görsel Efektler ===")]
    [Tooltip("Dash sýrasýnda aktif olacak partikül (opsiyonel)")]
    public ParticleSystem dashParticle;
    [Tooltip("Siren ýþýðý objesi (opsiyonel)")]
    public GameObject sirenLight;
    public AudioClip dashHornSound;
    public AudioClip hitSound;

    // Durum makinesi
    public enum State { Patrol, Chase, WindUp, Dash, Cooldown }
    [HideInInspector] public State currentState = State.Patrol;

    // Private
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private int currentPatrolIndex = 0;
    private float stateTimer = 0f;
    private Vector3 dashDirection;
    private Vector3 dashStartPos;
    private bool hasHitPlayerThisDash = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Player'ý tag ile bul
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        agent.speed = patrolSpeed;
        SetSiren(false);
    }

    void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        UpdateWheels();

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (distToPlayer < detectionRange)
                    ChangeState(State.Chase);
                break;

            case State.Chase:
                Chase();
                if (distToPlayer > detectionRange * 1.3f)
                    ChangeState(State.Patrol);
                else if (distToPlayer < dashTriggerRange)
                    ChangeState(State.WindUp);
                break;

            case State.WindUp:
                WindUp();
                break;

            case State.Dash:
                Dash();
                break;

            case State.Cooldown:
                Cooldown();
                break;
        }
    }

    // ==================== DURUMLAR ====================

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        if (agent.remainingDistance < waypointThreshold)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    void Chase()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed * 1.5f;
        agent.SetDestination(player.position);
        SetSiren(true);
    }

    void WindUp()
    {
        // Yerinde dur, oyuncuya doðru dön, uyarý ver
        agent.isStopped = true;

        // Oyuncuya doðru yavaþça dön
        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(State.Dash);
        }
    }

    void Dash()
    {
        // NavMesh kapalý, doðrudan ileri git
        agent.isStopped = true;
        transform.position += dashDirection * dashSpeed * Time.deltaTime;

        stateTimer -= Time.deltaTime;

        // Maksimum mesafe veya süre kontrolü
        float distFromStart = Vector3.Distance(transform.position, dashStartPos);
        if (stateTimer <= 0f || distFromStart > dashSpeed * dashDuration * 1.2f)
        {
            ChangeState(State.Cooldown);
        }
    }

    void Cooldown()
    {
        agent.isStopped = true;
        SetSiren(false);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            // NavMesh üzerine geri snap
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            ChangeState(State.Chase);
        }
    }

    // ==================== DURUM GEÇÝÞ ====================

    void ChangeState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Patrol:
                SetSiren(false);
                if (dashParticle != null) dashParticle.Stop();
                break;

            case State.Chase:
                SetSiren(true);
                break;

            case State.WindUp:
                stateTimer = dashWindupTime;
                // Korna çal
                if (dashHornSound != null)
                    audioSource.PlayOneShot(dashHornSound);
                break;

            case State.Dash:
                stateTimer = dashDuration;
                hasHitPlayerThisDash = false;
                dashStartPos = transform.position;
                // Oyuncunun þu anki pozisyonuna doðru dash
                dashDirection = (player.position - transform.position).normalized;
                dashDirection.y = 0;
                if (dashParticle != null) dashParticle.Play();
                break;

            case State.Cooldown:
                stateTimer = dashCooldown;
                if (dashParticle != null) dashParticle.Stop();
                break;
        }
    }

    // ==================== ÇARPIÞMA ====================

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        float damage;
        float force;

        if (currentState == State.Dash && !hasHitPlayerThisDash)
        {
            damage = dashDamage;
            force = knockbackForce;
            hasHitPlayerThisDash = true;
        }
        else
        {
            damage = normalHitDamage;
            force = knockbackForce * 0.4f;
        }

        // Hasar ver
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);

        // Fýrlatma kuvveti uygula
        Rigidbody playerRb = other.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 knockDir = (other.transform.position - transform.position).normalized;
            knockDir.y = 0;
            Vector3 totalForce = knockDir * force + Vector3.up * knockUpForce;
            playerRb.AddForce(totalForce, ForceMode.Impulse);
        }

        // Ses efekti
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);
    }

    // ==================== YARDIMCI ====================

    void UpdateWheels()
    {
        bool moving = agent.velocity.magnitude > 0.5f || currentState == State.Dash;
        float speed = currentState == State.Dash ? dashSpeed : agent.velocity.magnitude;

        foreach (var w in wheels)
        {
            if (w != null)
            {
                w.SetMoving(moving);
                w.SetSpeed(speed);
            }
        }
    }

    void SetSiren(bool active)
    {
        if (sirenLight != null)
            sirenLight.SetActive(active);
    }

    // Gizmos ile debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashTriggerRange);
    }
}

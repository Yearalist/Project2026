using UnityEngine;

namespace ToySiege.Enemy.Data
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "ToySiege/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("══ SAĞLIK ══")]
        public float MaxHealth = 50f;

        [Header("══ HAREKET ══")]
        public float MoveSpeed = 3.5f;
        public float ChaseSpeed = 5f;
        public float RotationSpeed = 720f;
        public float StoppingDistance = 1.5f;

        [Header("══ ALGILAMA ══")]
        public float DetectionRange = 12f;
        public float LoseRange = 18f;
        public float FieldOfView = 120f;

        [Header("══ SALDIRI ══")]
        public float AttackRange = 1.8f;
        public float AttackDamage = 10f;
        public float AttackCooldown = 1.2f;
        public float AttackDuration = 0.5f;

        [Header("══ IDLE ══")]
        public float IdleWaitMin = 1f;
        public float IdleWaitMax = 3f;
        public float PatrolRadius = 5f;
    }
}
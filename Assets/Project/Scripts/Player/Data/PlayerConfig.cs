

using UnityEngine;

namespace ToySiege.Player.Data
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "ToySiege/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("══ YÜRÜME / KOŞMA ══")]
        [Tooltip("W tuşu ile yürüme hızı")]
        public float WalkSpeed = 4f;

        [Tooltip("W + Shift ile koşma hızı")]
        public float SprintSpeed = 8f;

        [Header("══ MOUSE DÖNÜŞÜ ══")]
        [Tooltip("Mouse hassasiyeti — cursor kilitliyken 300-500 arası iyi çalışır")]
        public float MouseRotationSpeed = 400f;

        [Tooltip("Dönüşün yumuşaklığı (saniye). Düşük = keskin, Yüksek = yumuşak.\n0.05 = neredeyse anında\n0.1 = hafif smooth\n0.2 = belirgin smooth")]
        [Range(0.01f, 0.3f)]
        public float RotationSmoothTime = 0.08f;

        [Header("══ ZIPLAMA ══")]
        public float JumpForce = 12f;
        public float DoubleJumpForce = 9f;
        public float Gravity = -25f;
        public float CoyoteTime = 0.12f;
        public float JumpBufferTime = 0.1f;

        [Header("══ SLIDE ══")]
        public float SlideDuration = 0.45f;
        public float SlideSpeed = 14f;
        public float SlideCooldown = 0.5f;
        public float SlideColliderHeight = 0.8f;
        public float NormalColliderHeight = 2f;
        public float NormalColliderCenterY = 1f;
        public float SlideColliderCenterY = 0.4f;

        [Header("══ GENEL ══")]
        public float MaxHealth = 100f;
    }
}
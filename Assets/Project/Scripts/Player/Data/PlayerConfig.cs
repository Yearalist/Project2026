using UnityEngine;

namespace ToySiege.Player.Data
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "ToySiege/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("══ HAREKET ══")]
        [Tooltip("Koşma hızı (metre/saniye)")]
        public float MoveSpeed = 7f;

        [Tooltip("Karakterin dönüş hızı (derece/saniye). 720 = çok hızlı döner")]
        public float RotationSpeed = 720f;

        [Header("══ ZIPLAMA ══")]
        [Tooltip("İlk zıplama kuvveti")]
        public float JumpForce = 12f;

        [Tooltip("Double jump kuvveti (genelde ilkinden biraz düşük)")]
        public float DoubleJumpForce = 9f;

        [Tooltip("Yerçekimi ivmesi (negatif değer). -20 = hızlı düşüş, -9.8 = gerçekçi")]
        public float Gravity = -25f;

        [Tooltip("Kenardan düştükten sonra kaç saniye hâlâ zıplayabilir (Coyote Time)")]
        public float CoyoteTime = 0.12f;

        [Tooltip("Yere değmeden önce basılan jump tuşu kaç saniye geçerli kalır")]
        public float JumpBufferTime = 0.1f;

        [Header("══ SLIDE ══")]
        [Tooltip("Slide süresi (saniye)")]
        public float SlideDuration = 0.45f;

        [Tooltip("Slide sırasında hız (normal koşudan hızlı olmalı)")]
        public float SlideSpeed = 14f;

        [Tooltip("Ardışık slide'lar arası bekleme süresi")]
        public float SlideCooldown = 0.5f;

        [Tooltip("Slide sırasında collider yüksekliği (normal: 2, slide: 0.8)")]
        public float SlideColliderHeight = 0.8f;

        [Tooltip("Normal collider yüksekliği")]
        public float NormalColliderHeight = 2f;

        [Tooltip("Normal collider center Y")]
        public float NormalColliderCenterY = 1f;

        [Tooltip("Slide collider center Y")]
        public float SlideColliderCenterY = 0.4f;

        [Header("══ GENEL ══")]
        [Tooltip("Maksimum can")]
        public float MaxHealth = 100f;
    }
}
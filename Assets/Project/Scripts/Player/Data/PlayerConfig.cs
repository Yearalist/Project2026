using UnityEngine;

namespace ToySiege.Player.Data
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "ToySiege/Player Config")]
    public class PlayerConfig : ScriptableObject


    {
        [Header("══ YÜRÜME / KOŞMA ══")]
        public float WalkSpeed = 4f;
        public float SprintSpeed = 8f;

        [Header("══ MOUSE DÖNÜŞÜ ══")]
        public float MouseRotationSpeed = 400f;
        [Range(0.01f, 0.3f)]
        public float RotationSmoothTime = 0.08f;

        [Header("══ ZIPLAMA ══")]
        public float JumpForce = 12f;
        public float DoubleJumpForce = 9f;
        public float Gravity = -25f;
        public float CoyoteTime = 0.12f;
        public float JumpBufferTime = 0.1f;

        [Header("══ SLIDE ══")]
        public float SlideDuration = 1.533f;       // klip uzunluğu
        public float SlideMaxSpeed = 7f;           // en hızlı anındaki hız
        public float SlideCooldown = 0.5f;
       

        [Tooltip("X=zaman(0-1), Y=hız çarpanı(0-1). Animasyonla senkronize et.")]
        public AnimationCurve SlideSpeedCurve = new AnimationCurve(
            new Keyframe(0.0f, 0.2f),   // başlangıç: yavaş (yere iniyor)
            new Keyframe(0.2f, 1.0f),   // 0.3s sonra: tam hız (kayıyor)
            new Keyframe(0.7f, 1.0f),   // 1.0s'ye kadar: tam hız
            new Keyframe(1.0f, 0.0f)    // son: yavaşla ve dur (kalkıyor)
        );

        [Header("══ COLLIDER (CharacterController) ══")]
        [Tooltip("Normal duruşta collider yüksekliği — modelin boyuyla eşleş")]
        public float NormalColliderHeight = 1.8f;

        [Tooltip("Normal duruşta collider center Y — Height/2 olmalı")]
        public float NormalColliderCenterY = 0.9f;

        [Tooltip("Slide sırasında collider yüksekliği — en az 2*Radius (0.6) olmalı")]
        public float SlideColliderHeight = 1.0f;

        [Tooltip("Slide sırasında center Y — Height/2 olmalı ki alt kenar zeminde kalsın")]
        public float SlideColliderCenterY = 0.5f;

        [Header("══ GENEL ══")]
        public float MaxHealth = 100f;
    }
}

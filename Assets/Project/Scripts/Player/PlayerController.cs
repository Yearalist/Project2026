using UnityEngine;
using ToySiege.Core.FSM;
using ToySiege.Player.Data;
using ToySiege.Player.States;

namespace ToySiege.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerAnimator))]
    [RequireComponent(typeof(PlayerVFX))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Konfigürasyon")]
        [SerializeField] private PlayerConfig _config;

        // PUBLIC
        public PlayerConfig Config => _config;
        public PlayerInputHandler Input { get; private set; }
        public PlayerAnimator Anim { get; private set; }
        public PlayerVFX VFX { get; private set; }
        public StateMachine FSM { get; private set; }

        public bool IsGrounded => _cc.isGrounded;
        public float VerticalVelocity => _velocity.y;
        public bool HasDoubleJump { get; private set; }
        public bool CanSlide => _slideCooldownTimer <= 0f;
        public bool IsSprinting { get; set; }

        // PRIVATE
        private CharacterController _cc;
        private Vector3 _velocity;
        private float _slideCooldownTimer;
        private float _targetYaw;
        private float _currentYaw;
        private float _yawSmoothVelocity;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            Input = GetComponent<PlayerInputHandler>();
            Anim = GetComponent<PlayerAnimator>();
            VFX = GetComponent<PlayerVFX>();

            // ── DEBUG: Animator kontrolü ──
            var animator = GetComponentInChildren<Animator>();
            if (animator == null)
                Debug.LogError("[PlayerController] HATA: Animator BULUNAMADI! " +
                    "Model objesi Player'ın CHILD'ı olmalı ve üzerinde Animator component olmalı.");
            else if (animator.runtimeAnimatorController == null)
                Debug.LogError("[PlayerController] HATA: Animator Controller ATANMAMIŞ! " +
                    "Model objesindeki Animator'ın Controller slotuna PlayerAnimatorController sürükle.");
            else
                Debug.Log($"<color=green>[PlayerController] Animator OK: {animator.runtimeAnimatorController.name}</color>");

            FSM = new StateMachine();
            var factory = new PlayerStateFactory(this);
            FSM.Initialize(factory.Idle());

            FSM.OnStateChanged += (prev, next) =>
            {
                Debug.Log($"<color=yellow>[FSM] {prev?.GetType().Name ?? "None"} → {next.GetType().Name}</color>");
            };

            _targetYaw = transform.eulerAngles.y;
            _currentYaw = _targetYaw;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            FSM.Update();

            if (_slideCooldownTimer > 0f)
                _slideCooldownTimer -= Time.deltaTime;

            if (IsGrounded && VerticalVelocity <= 0f)
                HasDoubleJump = true;

            Anim.SetGrounded(IsGrounded);

            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
                else
                { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            }
        }

        private void FixedUpdate() => FSM.FixedUpdate();

        // ── Mouse Rotation ──
        public void HandleMouseRotation()
        {
            float mouseX = Input.MouseX;
            _targetYaw += mouseX * _config.MouseRotationSpeed * Time.fixedDeltaTime;
            _currentYaw = Mathf.SmoothDampAngle(
                _currentYaw, _targetYaw,
                ref _yawSmoothVelocity, _config.RotationSmoothTime
            );
            transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
        }

        // ── Hareket ──
        public void HandleWalkMovement()
        {
            Vector3 moveDir = GetCharacterRelativeDirection();
            _velocity.x = moveDir.x * _config.WalkSpeed;
            _velocity.z = moveDir.z * _config.WalkSpeed;
        }

        public void HandleSprintMovement()
        {
            Vector3 moveDir = GetCharacterRelativeDirection();
            _velocity.x = moveDir.x * _config.SprintSpeed;
            _velocity.z = moveDir.z * _config.SprintSpeed;
        }

        public void HandleAirMovement()
        {
            Vector3 moveDir = GetCharacterRelativeDirection();
            float speed = IsSprinting ? _config.SprintSpeed : _config.WalkSpeed;
            _velocity.x = moveDir.x * speed;
            _velocity.z = moveDir.z * speed;
        }

        // ── Fizik ──
        public void ApplyGravity()
        {
            if (IsGrounded && _velocity.y < 0f)
                _velocity.y = -2f;
            else
                _velocity.y += _config.Gravity * Time.fixedDeltaTime;
        }

        public void MoveCharacter() => _cc.Move(_velocity * Time.fixedDeltaTime);

        // ── Velocity ──
        public void SetVerticalVelocity(float value) => _velocity.y = value;
        public void SetHorizontalVelocity(Vector3 h) { _velocity.x = h.x; _velocity.z = h.z; }

        // ── Double Jump ──
        public void ResetDoubleJump() => HasDoubleJump = true;
        public void ConsumeDoubleJump() => HasDoubleJump = false;

        // ── Slide ──
        public void StartSlideCooldown() => _slideCooldownTimer = _config.SlideCooldown;

        /// <summary>
        /// Collider yüksekliğini GÜVENLİ şekilde değiştirir.
        /// 
        /// SORUN: Height küçültülürken capsule'ün alt noktası
        /// zeminin altına inebilir → karakter düşer.
        /// 
        /// ÇÖZÜM: Önce yeni bottom pozisyonunu hesapla.
        /// Eğer mevcut bottom'dan farklıysa, karakteri yukarı it
        /// ki capsule'ün altı hep aynı yerde (zemin hizasında) kalsın.
        /// </summary>
        public void SetColliderHeight(float newHeight, float newCenterY)
        {
            // Mevcut capsule'ün alt noktası
            float currentBottom = _cc.center.y - _cc.height / 2f;

            // Yeni capsule'ün alt noktası
            float newBottom = newCenterY - newHeight / 2f;

            // Fark kadar karakteri yukarı/aşağı kaydır
            float bottomDiff = currentBottom - newBottom;

            // Önce height ve center'ı değiştir
            _cc.height = newHeight;
            _cc.center = new Vector3(0f, newCenterY, 0f);

            // Sonra pozisyonu düzelt — capsule altı aynı yerde kalsın
            if (Mathf.Abs(bottomDiff) > 0.01f)
            {
                transform.position += new Vector3(0f, bottomDiff, 0f);
            }
        }

        // ── Yardımcı ──
        private Vector3 GetCharacterRelativeDirection()
        {
            Vector2 input = Input.MoveInput;
            if (input.sqrMagnitude < 0.01f) return Vector3.zero;

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            forward.y = 0f; right.y = 0f;
            forward.Normalize(); right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }

        private void OnDrawGizmosSelected()
        {
            if (_cc == null) return;

            // CharacterController capsule'ünü görselleştir
            Gizmos.color = Color.green;
            Vector3 center = transform.position + _cc.center;
            float halfHeight = _cc.height / 2f;
            Gizmos.DrawWireSphere(center + Vector3.up * (halfHeight - _cc.radius), _cc.radius);
            Gizmos.DrawWireSphere(center - Vector3.up * (halfHeight - _cc.radius), _cc.radius);

            // İleri yön
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * 2f);
        }
    }
}
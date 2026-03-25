using UnityEngine;
using ToySiege.Core.FSM;
using ToySiege.Player.Data;
using ToySiege.Player.States;

namespace ToySiege.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerAnimator))]
    public class PlayerController : MonoBehaviour
    {
        // ══════════════════════════════════════════
        // INSPECTOR
        // ══════════════════════════════════════════

        [Header("Konfigürasyon")]
        [Tooltip("ScriptableObject: Assets/_Project/ScriptableObjects/Player/ içinden sürükle")]
        [SerializeField] private PlayerConfig _config;

        // ══════════════════════════════════════════
        // PUBLIC PROPERTY'LER (State'ler bunları okur)
        // ══════════════════════════════════════════

        public PlayerConfig Config => _config;
        public PlayerInputHandler Input { get; private set; }
        public PlayerAnimator Anim { get; private set; }
        public StateMachine FSM { get; private set; }

        private float _turnSmoothVelocity; // Dönüş yumuşatması için gerekli referans

        /// <summary>Karakter yere değiyor mu?</summary>
        public bool IsGrounded => _cc.isGrounded;

        /// <summary>Dikey hız (pozitif = yukarı, negatif = düşüş)</summary>
        public float VerticalVelocity => _velocity.y;

        /// <summary>Double jump hakkı var mı?</summary>
        public bool HasDoubleJump { get; private set; }

        /// <summary>Slide yapabilir mi? (cooldown kontrolü)</summary>
        public bool CanSlide => _slideCooldownTimer <= 0f;

        // ══════════════════════════════════════════
        // PRIVATE
        // ══════════════════════════════════════════

        private CharacterController _cc;
        private Vector3 _velocity;           // Mevcut hareket vektörü
        private float _slideCooldownTimer;
        private Camera _mainCamera;

        // ══════════════════════════════════════════
        // UNITY LIFECYCLE
        // ══════════════════════════════════════════

        private void Awake()
        {
            // Component referansları
            _cc = GetComponent<CharacterController>();
            Input = GetComponent<PlayerInputHandler>();
            Anim = GetComponent<PlayerAnimator>();
            _mainCamera = Camera.main;

            // State Machine başlat
            FSM = new StateMachine();
            var factory = new PlayerStateFactory(this);
            FSM.Initialize(factory.Idle());

            // Debug: State değişimlerini logla
            FSM.OnStateChanged += (prev, next) =>
            {
                Debug.Log($"[FSM] {prev?.GetType().Name ?? "None"} → {next.GetType().Name}");
            };
        }

        private void Update()
        {
            // State Machine her frame çalışsın (input kontrol, geçişler)
            FSM.Update();

            // Cooldown timer'ları güncelle
            if (_slideCooldownTimer > 0f)
                _slideCooldownTimer -= Time.deltaTime;

            // Yere inince double jump hakkını yenile
            // (GDD: "Karakter yere değince hak yenilenir")
            if (IsGrounded && VerticalVelocity <= 0f)
                HasDoubleJump = true;

            // Animator'a grounded bilgisi
            Anim.SetGrounded(IsGrounded);
        }

        private void FixedUpdate()
        {
            // State Machine fizik hesaplamaları
            FSM.FixedUpdate();
        }

        // ══════════════════════════════════════════
        // HAREKET API'Sİ (State'ler bunları çağırır)
        // ══════════════════════════════════════════

        /// <summary>
        /// Yerdeyken hareket. Kamera yönüne göre hesaplanır.
        /// GDD: "Kamera hangi yöne bakıyorsa karakter de ona göre hareket eder"
        /// </summary>
        public void HandleGroundMovement()
        {
            Vector3 moveDir = GetCameraRelativeDirection();

            if (moveDir.sqrMagnitude > 0.01f)
            {
                // Karakteri hareket yönüne döndür
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRot,
                    _config.RotationSpeed * Time.fixedDeltaTime
                );
            }

            // Yatay hızı ayarla
            _velocity.x = moveDir.x * _config.MoveSpeed;
            _velocity.z = moveDir.z * _config.MoveSpeed;
        }

        /// <summary>
        /// Havadayken hareket. Yerdekiyle aynı ama ileride
        /// air control katsayısı eklenebilir (örn: %70 kontrol).
        /// </summary>
        public void HandleAirMovement()
        {
            Vector3 moveDir = GetCameraRelativeDirection();

            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRot,
                    _config.RotationSpeed * Time.fixedDeltaTime
                );
            }

            // Havada biraz daha az kontrol (opsiyonel: * 0.7f)
            _velocity.x = moveDir.x * _config.MoveSpeed;
            _velocity.z = moveDir.z * _config.MoveSpeed;
        }

        /// <summary>
        /// Yerçekimi uygula.
        /// Yerdeyken küçük negatif değer (yapışma) — havadayken ivmelenme.
        /// </summary>
        public void ApplyGravity()
        {
            if (IsGrounded && _velocity.y < 0f)
            {
                // Yerdeyken hafif aşağı çekme (rampalarda kayma önlenir)
                _velocity.y = -2f;
            }
            else
            {
                _velocity.y += _config.Gravity * Time.fixedDeltaTime;
            }
        }

        /// <summary>
        /// Hesaplanan velocity'yi CharacterController'a uygula.
        /// TÜM hareket bu metod üzerinden gerçekleşir.
        /// </summary>
        public void MoveCharacter()
        {
            _cc.Move(_velocity * Time.fixedDeltaTime);
        }

        // ══════════════════════════════════════════
        // VELOCİTY KONTROL
        // ══════════════════════════════════════════

        /// <summary>Dikey hızı ayarla (jump force)</summary>
        public void SetVerticalVelocity(float value)
        {
            _velocity.y = value;
        }

        /// <summary>Yatay hızı ayarla (slide, durma)</summary>
        public void SetHorizontalVelocity(Vector3 horizontal)
        {
            _velocity.x = horizontal.x;
            _velocity.z = horizontal.z;
        }

        // ══════════════════════════════════════════
        // DOUBLE JUMP YÖNETİMİ
        // ══════════════════════════════════════════

        /// <summary>Double jump hakkını yenile (zıplama başında)</summary>
        public void ResetDoubleJump() => HasDoubleJump = true;

        /// <summary>Double jump hakkını tüket</summary>
        public void ConsumeDoubleJump() => HasDoubleJump = false;

        // ══════════════════════════════════════════
        // SLIDE YÖNETİMİ
        // ══════════════════════════════════════════

        /// <summary>Slide cooldown başlat</summary>
        public void StartSlideCooldown()
        {
            _slideCooldownTimer = _config.SlideCooldown;
        }

        /// <summary>
        /// Collider yüksekliğini değiştir.
        /// Slide sırasında küçült, çıkışta büyüt.
        /// GDD: "Slide sırasında karakterin boyu küçülecek"
        /// </summary>
        public void SetColliderHeight(float height, float centerY)
        {
            _cc.height = height;
            _cc.center = new Vector3(0f, centerY, 0f);
        }

        // ══════════════════════════════════════════
        // YARDIMCI METODLAR
        // ══════════════════════════════════════════

        /// <summary>
        /// Input'u kamera yönüne göre dünya koordinatına çevirir.
        /// W tuşu = kameranın baktığı yönde ileri.
        /// GDD: "Kamera hangi yöne bakıyorsa karakter de ona göre hareket eder"
        /// </summary>
        private Vector3 GetCameraRelativeDirection()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return Vector3.zero;
            }

            Transform camT = _mainCamera.transform;

            // Kameranın ileri ve sağ vektörleri (Y ekseni sıfırlanır — düz zemin)
            Vector3 forward = camT.forward;
            Vector3 right = camT.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // Input ile birleştir
            Vector2 input = Input.MoveInput;
            return (forward * input.y + right * input.x);
        }

        // ══════════════════════════════════════════
        // DEBUG (Sahne görünümünde yardımcı çizgiler)
        // ══════════════════════════════════════════

        private void OnDrawGizmosSelected()
        {
            // Hareket yönünü göster
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * 2f);

            // Hız vektörünü göster
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + Vector3.up,
                new Vector3(_velocity.x, 0, _velocity.z).normalized * 2f);
        }
    }
}

using UnityEngine;

namespace Retro.ThirdPersonCharacter
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Combat))]
    [RequireComponent(typeof(Rigidbody))]
    public class Movement : MonoBehaviour
    {
        private Animator _animator;
        private PlayerInput _playerInput;
        private Combat _combat;
        private Rigidbody _rigidbody;

        private Vector2 lastMovementInput;

        public float MaxSpeed = 5f;
        public float jumpForce = 5f;

        [Header("Ground Check")]
        public float groundCheckDistance = 0.3f;
        public LayerMask groundLayer = ~0; // Default: all layers

        public bool IsGrounded { get; private set; }

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _playerInput = GetComponent<PlayerInput>();
            _combat = GetComponent<Combat>();
            _rigidbody = GetComponent<Rigidbody>();

            _rigidbody.freezeRotation = true; // Prevent character from tipping over
        }

        private void Update()
        {
            if (_animator == null) return;

            // Ground check via raycast from slightly above feet
            IsGrounded = Physics.Raycast(
                transform.position + Vector3.up * 0.1f,
                Vector3.down,
                groundCheckDistance,
                groundLayer
            );

            if (_combat.AttackInProgress)
                StopMovementOnAttack();
            else
                Move();
        }

        private void Move()
        {
            var x = _playerInput.MovementInput.x;
            var y = _playerInput.MovementInput.y;

            Vector3 moveDir = new Vector3(x, 0, y);
            moveDir = transform.TransformDirection(moveDir);
            moveDir *= MaxSpeed;

            // Apply horizontal movement, preserve vertical (gravity from Rigidbody)
            _rigidbody.linearVelocity = new Vector3(moveDir.x, _rigidbody.linearVelocity.y, moveDir.z);

            // Jump
            if (IsGrounded && _playerInput.JumpInput)
            {
                _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
                _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

            lastMovementInput = new Vector2(x, y);

            _animator.SetFloat("InputX", x);
            _animator.SetFloat("InputY", y);
            _animator.SetBool("IsInAir", !IsGrounded);
        }

        private void StopMovementOnAttack()
        {
            // Stop horizontal movement while attacking, preserve gravity
            _rigidbody.linearVelocity = new Vector3(0, _rigidbody.linearVelocity.y, 0);

            _animator.SetFloat("InputX", 0);
            _animator.SetFloat("InputY", 0);
        }
    }
}
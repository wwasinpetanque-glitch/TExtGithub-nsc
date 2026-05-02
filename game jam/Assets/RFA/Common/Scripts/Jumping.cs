using UnityEngine;

namespace Retro.ThirdPersonCharacter
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Combat))]
    public class Jumping : MonoBehaviour
    {
        private Animator _animator;
        private PlayerInput _playerInput;
        private Movement _movement;

        private bool _wasGrounded = true;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _playerInput = GetComponent<PlayerInput>();
            _movement = GetComponent<Movement>();
        }

        private void Update()
        {
            bool grounded = _movement.IsGrounded;

            // Fire "Jump" trigger at the exact moment player leaves the ground
            if (_wasGrounded && !grounded && _playerInput.JumpInput)
            {
                _animator.SetTrigger("Jump");
            }

            _wasGrounded = grounded;
        }
    }
}
using UnityEngine;
using NaughtyCharacter;
using System.Collections;

namespace Retro.ThirdPersonCharacter
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(Animator))]
    public class Combat : MonoBehaviour
    {
        private const string attackTriggerName = "Attack";
        private const string specialAttackTriggerName = "Ability";

        private Animator _animator;
        private PlayerInput _playerInput;
        private Coroutine _attackFallbackCoroutine;

        [Tooltip("Fallback time to reset AttackInProgress if Animation Events are not set up")]
        public float attackDuration = 0.8f;

        public bool AttackInProgress { get; private set; } = false;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _playerInput = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            if(_playerInput.AttackInput && !AttackInProgress)
            {
                Attack();
            }
            else if (_playerInput.SpecialAttackInput && !AttackInProgress)
            {
                SpecialAttack();
            }
        }

        // Called via Animation Event at START of attack clip
        public void SetAttackStart()
        {
            AttackInProgress = true;
        }

        // Called via Animation Event at END of attack clip
        public void SetAttackEnd()
        {
            AttackInProgress = false;
            if (_attackFallbackCoroutine != null)
            {
                StopCoroutine(_attackFallbackCoroutine);
                _attackFallbackCoroutine = null;
            }
        }

        private void Attack()
        {
            _animator.SetTrigger(attackTriggerName);
            if (_attackFallbackCoroutine != null) StopCoroutine(_attackFallbackCoroutine);
            _attackFallbackCoroutine = StartCoroutine(AttackFallback());
        }

        private void SpecialAttack()
        {
            _animator.SetTrigger(specialAttackTriggerName);
            if (_attackFallbackCoroutine != null) StopCoroutine(_attackFallbackCoroutine);
            _attackFallbackCoroutine = StartCoroutine(AttackFallback());
        }

        // Fallback: auto-resets AttackInProgress if no Animation Events are configured
        private IEnumerator AttackFallback()
        {
            AttackInProgress = true;
            yield return new WaitForSeconds(attackDuration);
            AttackInProgress = false;
            _attackFallbackCoroutine = null;
        }
    }
}
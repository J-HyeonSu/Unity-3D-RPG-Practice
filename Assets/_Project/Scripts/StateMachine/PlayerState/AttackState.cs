using UnityEngine;

namespace RpgPractice
{
    public class AttackState : BaseState
    {
        private bool isCombo;
        public AttackState(PlayerController player, Animator animator) : base(player, animator)
        {
        }

        public override void OnEnter()
        {
            isCombo = false;
            player.ChangeAttackState += OnAttack;
            OnAttack(player.attackNum);
        }

        public override void OnExit()
        {
            player.ChangeAttackState -= OnAttack;
        }

        private void OnAttack(int value)
        {
            if (value != 0) isCombo = false;
            switch (value)
            {
                case 0 :
                    if (!isCombo)
                    {
                        animator.CrossFade(LeftAttack1Hash, crossFadeDuration);
                        isCombo = true;
                    }
                    else
                    {
                        animator.CrossFade(LeftAttack2Hash, crossFadeDuration);
                        isCombo = false;
                    }
                    break;
                case 1:
                    animator.CrossFade(RightAttackHash, crossFadeDuration);
                    break;
                case 2:
                    animator.CrossFade(BlockHash, crossFadeDuration);
                    break;
                case 3:
                    animator.CrossFade(RushAttackHash, crossFadeDuration);
                    break;
            }
            
        }

        public override void Update()
        {
            
        }


        public override void FixedUpdate()
        {
            player.HandleMovement();
            player.HandleJump();
        }
    }
}
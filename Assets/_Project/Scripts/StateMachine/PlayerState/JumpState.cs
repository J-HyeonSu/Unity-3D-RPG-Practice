﻿using UnityEngine;

namespace RpgPractice
{
    public class JumpState : BaseState
    {
        public JumpState(PlayerController player, Animator animator) : base(player, animator)
        {
        }

        public override void OnEnter()
        {
            animator.CrossFade(JumpHash, crossFadeDuration);
        }

        public override void FixedUpdate()
        {
            player.HandleMovement();
            player.HandleJump();
        }
    }
}
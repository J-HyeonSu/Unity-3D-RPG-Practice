using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Random = UnityEngine.Random;

namespace RpgPractice
{
    public class GolemBoss : Enemy
    {
        enum GolemAttackPattern { JumpAttack, RightAttack, LeftAttack, Roaring }

        [SerializeField] private BossAttackTelegraph bossAttackTelegraph;
        [SerializeField] private BossAttackData[] bossAttackDatas;
        [SerializeField] private Transform model;
        
        private GolemAttackPattern lastAttack;
        
        //debug
        private Vector3 ct;
        private float rad;
        protected override void Start()
        {
            SetupStateMachine();
            bossAttackTelegraph.OnTelegraphComplete += BossAttackTelegraph_OnTelegraphComplete;
        }

        private void BossAttackTelegraph_OnTelegraphComplete(BossAttackData bossAttackData, Vector3 center, Vector3 forward)
        {
            var enemyList = AttackRangeDetector.GetTargetsInRange(bossAttackData, center, forward, LayerMask.GetMask("Character"));
            
            foreach (var transform in enemyList)
            {
                if (transform.gameObject.CompareTag("Player"))
                {
                    transform.gameObject.GetComponent<Health>().TakeDamage(bossAttackData.damage);
                }
            }
        }

        private void OnDrawGizmos()
        {
            //Gizmos.DrawWireSphere(ct, rad);
        }


        protected override void SetupStateMachine()
        {
            //보스용 상태머신
            stateMachine = new StateMachine();
            attackTimer = new CountdownTimer(timeBetweenAttacks);

            attackTimer.OnTimerStart += () => { canMove = false; };
            attackTimer.OnTimerStop += () => { canMove = true; };

            var locomotionState = new EnemyLocomotionState(this, animator, agent, wanderRadius, playerDetector.Player);
            var attackState = new EnemyAttackState(this, animator, agent, playerDetector.Player);
            var deadState = new EnemyDeadState(this, animator);

            At(locomotionState, attackState, new FuncPredicate(()=> playerDetector.CanAttackPlayer()));
            At(attackState, locomotionState, new FuncPredicate(()=> canMove));
            Any(deadState, new FuncPredicate(()=> GetComponent<Health>().IsDead));
            
            stateMachine.SetState(locomotionState);
            
        }

        public override void Attack()
        {
            if (attackTimer.IsRunning) return;
            if (health.IsDead) return;
            attackTimer.Start();
            
            GolemAttackPattern currentAttack = 0;
            while (lastAttack != currentAttack)
            {
                currentAttack = (GolemAttackPattern)Random.Range(0, 3);
            }
            
            //점프공격 테스트용
            currentAttack = 0;

            switch (currentAttack)
            {
                default:
                case GolemAttackPattern.JumpAttack:
                    JumpAttackStart(bossAttackDatas[(int)currentAttack]);
                    break;
                case GolemAttackPattern.RightAttack:
                    break;
                case GolemAttackPattern.LeftAttack:
                    break;
                case GolemAttackPattern.Roaring:
                    break;
                
            }
            

        }

        private void JumpAttackStart(BossAttackData currentAttack)
        {
            animator.SetTrigger("JumpAttack");
            bossAttackTelegraph.ShowTelegraph(currentAttack, transform.position, transform.forward);

            StartCoroutine(JumpCoroutine(5.0f, currentAttack.castTime));
        }

        IEnumerator JumpCoroutine(float height, float totalTime)
        {
            Vector3 startPosition = transform.position;
            float halfTime = totalTime * 0.5f; // 올라가는 시간 = 내려가는 시간
    
            // 올라가기
            float elapsedTime = 0f;
            while (elapsedTime < halfTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / halfTime;
                float currentHeight = Mathf.Lerp(0f, height, progress);
        
                transform.position = startPosition + Vector3.up * currentHeight;
                yield return null;
            }
    
            // 내려가기
            elapsedTime = 0f;
            while (elapsedTime < halfTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / halfTime;
                float currentHeight = Mathf.Lerp(height, 0f, progress);
        
                transform.position = startPosition + Vector3.up * currentHeight;
                yield return null;
            }
    
            // 정확한 착지 위치 보정
            transform.position = startPosition;
            
        }
    }
}
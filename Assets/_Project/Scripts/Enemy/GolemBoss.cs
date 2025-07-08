using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Random = UnityEngine.Random;

namespace RpgPractice
{
    public class GolemBoss : Enemy
    {
        enum GolemAttackPattern { JumpAttack, RightAttack, LeftAttack, Roaring }

        //[SerializeField] private BossAttackTelegraph bossAttackTelegraph;
        [SerializeField] private BossAttackData[] bossAttackDatas;
        [SerializeField] private GameObject telegraphPrefab;
        
        
        private GolemAttackPattern lastAttack;
        
        //debug
        public bool debugAttack;    
        public int testAttackNum = 0;
        private Vector3 ct;
        private float rad;
        protected override void Start()
        {
            SetupStateMachine();
            //bossAttackTelegraph.OnTelegraphComplete += BossAttackTelegraph_OnTelegraphComplete;
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

        private void ShowTelegraphWithCallback(BossAttackData currentData, Vector3 center, Vector3 forward)
        {
            var telegraphObj = PoolManager.instance.Get(telegraphPrefab);
            var telegraph = telegraphObj.GetComponent<BossAttackTelegraph>();

            if (telegraph)
            {
                telegraph.OnTelegraphComplete += BossAttackTelegraph_OnTelegraphComplete;
                telegraph.ShowTelegraph(currentData, center, forward);
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
            
            
            GolemAttackPattern currentAttack = (GolemAttackPattern)Random.Range(0, 3);
            while (lastAttack == currentAttack)
            {
                currentAttack = (GolemAttackPattern)Random.Range(0, 3);
            }
            
            lastAttack = currentAttack;
            
            //공격 테스트용
            //currentAttack = (GolemAttackPattern)0;
            if (debugAttack)
            {
                currentAttack = (GolemAttackPattern)testAttackNum;
            }
            
            
            switch (currentAttack)
            {
                case GolemAttackPattern.JumpAttack:
                    //대상 점프 공격
                    JumpAttackStart(bossAttackDatas[(int)currentAttack]);
                    break;
                case GolemAttackPattern.RightAttack:
                    //오른손 전방 180도 부채꼴
                    ShowTelegraphWithCallback(bossAttackDatas[(int)currentAttack], transform.position, transform.forward);
                    animator.SetTrigger("RightAttack");
                    break;
                case GolemAttackPattern.LeftAttack:
                    //왼손 부채꼴 90도 왼쪽으로 30도 틀어서 왼,앞만 맞게
                    ShowTelegraphWithCallback(bossAttackDatas[(int)currentAttack], transform.position, Quaternion.AngleAxis(-30f, Vector3.up) * transform.forward);
                    animator.SetTrigger("LeftAttack");
                    break;
                case GolemAttackPattern.Roaring:
                    RoaringAttackStart(bossAttackDatas[(int)currentAttack]);

                    break;
                
            }
            
        }

        private void RoaringAttackStart(BossAttackData currentAttack)
        {
            //전방레이저
            
            animator.SetTrigger("Roaring");
            //레이저 관련 설정
            StartCoroutine(RoaringCoroutine(currentAttack));
        }

        IEnumerator RoaringCoroutine(BossAttackData currentAttack)
        {
            float[] progressPoints = { 0.25f, 0.5f, 0.75f };
    
            for (int i = 0; i < progressPoints.Length; i++)
            {
                float waitTime = currentAttack.castTime * progressPoints[i];
                if (i > 0)
                    waitTime -= currentAttack.castTime * progressPoints[i-1]; // 누적 시간 빼기
        
                yield return new WaitForSeconds(waitTime);
                ShowTelegraphWithCallback(currentAttack, transform.position, Quaternion.AngleAxis(30f*i-30, Vector3.up) * transform.forward);
            }
            
            yield return new WaitForSeconds(currentAttack.castTime * 0.25f);
        }

        private void JumpAttackStart(BossAttackData currentAttack)
        {
            //공격위치, 방법
            ShowTelegraphWithCallback(currentAttack, playerDetector.Player.position, transform.forward);
            //bossAttackTelegraph.ShowTelegraph(currentAttack, playerDetector.Player.position, transform.forward);
            
            //애니메이션 처리
            animator.SetTrigger("JumpAttack");
            StartCoroutine(JumpCoroutine(6.0f, currentAttack.castTime, playerDetector.Player.position));
        }

        IEnumerator JumpCoroutine(float height, float totalTime, Vector3 targetPosition)
        {
            var startPosition = transform.position;
    
            // 보스의 Collider 크기 가져오기
            Collider bossCollider = GetComponent<Collider>();
            float bossRadius = bossCollider.bounds.size.x * 0.5f;
            
            Vector3 direction = (targetPosition - startPosition).normalized;
            Vector3 actualTarget = targetPosition - direction * bossRadius;
            
            float elapsedTime = 0f;
            while (elapsedTime < totalTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / totalTime;

                float jumpProgress = Mathf.Sin(progress * Mathf.PI);
                float currentHeight = jumpProgress * height;
                
                Vector3 horizontalPosition = Vector3.Lerp(startPosition, actualTarget, progress);
        
                // // 현재 Y좌표는 애니메이션이 처리하고 있으므로 유지
                // float currentY = transform.position.y;
                //
                // // X, Z만 업데이트하고 Y는 애니메이션 그대로
                // transform.position = new Vector3(horizontalPosition.x, currentY, horizontalPosition.z);
                
                transform.position = horizontalPosition + Vector3.up * currentHeight;
                yield return null;
            }
    
            // 정확한 착지 위치 보정
            transform.position = targetPosition;

        }
    }
}
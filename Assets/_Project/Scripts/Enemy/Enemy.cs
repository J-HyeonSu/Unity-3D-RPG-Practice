using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace RpgPractice
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PlayerDetector))]
    public class Enemy : Entity
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private PlayerDetector playerDetector;
        [SerializeField] private Animator animator;
        [SerializeField] private float wanderRadius = 10;
        [SerializeField] private float timeBetweenAttacks = 1f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private bool isRanged;
        [SerializeField] private float deadTime = 10f;
        [SerializeField] private ProjectileData projectileData;

        private StateMachine stateMachine;
        public CountdownTimer attackTimer;

        public bool isDetected;
        private bool canMove;

        void Start()
        {
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

        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        void Update()
        {
            stateMachine.Update();
            
            if (playerDetector.CanDetectPlayer() && !isDetected)
            {
                isDetected = true;
                agent.speed += 1;
            }
            else if(!playerDetector.CanDetectPlayer() && isDetected)
            {
                isDetected = false;
                agent.speed -= 1;
            }
            
            animator.SetFloat("Speed", agent.velocity.magnitude);
            
            attackTimer.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        public void Attack()
        {
            if (attackTimer.IsRunning) return;

            attackTimer.Start();
            
            //공격 로직
            if (isRanged)
            {
                if (!projectileData) return;
                var proj = PoolManager.instance.Get(projectileData.prefab);
                var direction = playerDetector.Player.position - transform.position;
                proj.GetComponentInChildren<Projectile>().Init(gameObject, transform.position, direction, projectileData, attackDamage);
                

            }
            else
            {
                playerDetector.PlayerHealth.TakeDamage(attackDamage);    
            }
            
            
        }

        public void AttackEnd()
        {
            canMove = true;
        }

        public void AlertObservers()
        {
            //Debug.Log("다른 적들에게 알림!");
            // 나중에 주변 적들에게 플레이어 위치 알려주는 로직
        }

        
        public void Dead()
        {
            agent.enabled = false;
            StartCoroutine(DeadCoroutine());

            //Destroy(gameObject);
        }

        IEnumerator DeadCoroutine()
        {
            yield return new WaitForSeconds(10f);
            Destroy(gameObject);
        }
        
    }
}
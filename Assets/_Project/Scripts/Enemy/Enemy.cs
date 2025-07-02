using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace RpgPractice
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PlayerDetector))]
    public abstract class Enemy : Entity
    {
        
        [Header("AI Components")]
        [SerializeField] protected NavMeshAgent agent;
        [SerializeField] protected PlayerDetector playerDetector;
        
        [Header("Enemy Settings")]        
        [SerializeField] protected float wanderRadius = 10;
        [SerializeField] protected float timeBetweenAttacks = 1f;
        [SerializeField] protected int attackDamage = 10;
        
        [SerializeField] protected float destroyTime = 10f;
        

        protected StateMachine stateMachine;
        public CountdownTimer attackTimer;

        
        protected bool canMove;

        protected virtual void Start()
        {
            SetupStateMachine();
        }

        protected virtual void SetupStateMachine()
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

        protected void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        protected void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        void Update()
        {
            stateMachine.Update();
            attackTimer.Tick(Time.deltaTime);
            
            // if (playerDetector.CanDetectPlayer() && !isDetected)
            // {
            //     isDetected = true;
            //     agent.speed += 1;
            // }
            // else if(!playerDetector.CanDetectPlayer() && isDetected)
            // {
            //     isDetected = false;
            //     agent.speed -= 1;
            // }
            //
            // animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public abstract void Attack();

        public void AttackEnd()
        {
            canMove = true;
        }

        public void AlertObservers()
        {
            //Debug.Log("다른 적들에게 알림!");
            // 나중에 주변 적들에게 플레이어 위치 알려주는 로직
        }
        

        
        public override void Die()
        {
            agent.enabled = false;
            StartCoroutine(DieCoroutine());
        }
        IEnumerator DieCoroutine()
        {
            yield return new WaitForSeconds(destroyTime);
            Destroy(gameObject);
        }


        public override void TakeDamage(float damage)
        {
            health.TakeDamage(damage);
        }
    }
}
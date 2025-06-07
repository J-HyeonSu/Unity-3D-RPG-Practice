using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Utilities;

namespace RpgPractice
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Animator animator;
        [SerializeField] private CinemachineFreeLook freeLookVCam;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private GroundChecker groundChecker;
        [SerializeField] public WeaponManager weaponManager;
        
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float smoothTime = 0.2f;

        [Header("Jump Setting")] 
        [SerializeField] float jumpForce = 10f;
        [SerializeField] float jumpDuration = 0.5f;
        [SerializeField] float jumpCooldown = 0f;
        [SerializeField] float gravityMultiplier = 3f;
        
        [Header("Attack Setting")] 
        [SerializeField] float attackCooldown = 0.5f;
        [SerializeField] float attackDistance = 3f;
        [SerializeField] float attackDamage = 10f;

        private StateMachine stateMachine;

        
        
        
        private const float ZeroF = 0f;

        private Transform mainCam;
        
        private float currentSpeed;
        private float velocity;
        private float jumpVelocity;
        private Vector3 adjustedDirection;

        private Vector3 movement;

        private List<Timer> timers;
        private CountdownTimer jumpTimer;
        private CountdownTimer jumpCooldownTimer;
        private CountdownTimer attackTimer;
        
        //animator parameters
        private static readonly int Speed = Animator.StringToHash("Speed");

        void Awake()
        {
            mainCam = Camera.main.transform;
            freeLookVCam.Follow = transform;
            freeLookVCam.LookAt = transform;
            freeLookVCam.OnTargetObjectWarped(
                transform,
                transform.position - freeLookVCam.transform.position - Vector3.forward);

            rb.freezeRotation = true;
            
            SetupTimers();

            SetupStateMachine();
        }

        private void SetupStateMachine()
        {
            stateMachine = new StateMachine();

            var locomotionState = new LocomotionState(this, animator);
            var jumpState = new JumpState(this, animator);
            var attackState = new AttackState(this, animator);

            At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
            At(locomotionState, attackState, new FuncPredicate(() => attackTimer.IsRunning));
            At(attackState, locomotionState, new FuncPredicate(() => !attackTimer.IsRunning));
            
            Any(locomotionState, new FuncPredicate(ReturnToLocomotionState));

            
            stateMachine.SetState(locomotionState);
        }

        bool ReturnToLocomotionState()
        {
            //기본애니메이션
            return groundChecker.IsGrounded
                   && !jumpTimer.IsRunning
                   && !attackTimer.IsRunning;
        }

        void At(IState from, IState to, IPredicate condition)
        {
            stateMachine.AddTransition(from, to, condition);
        }

        void Any(IState to, IPredicate condition)
        {
            stateMachine.AddAnyTransition(to, condition);
        }

        void SetupTimers()
        {
            jumpTimer = new CountdownTimer(jumpDuration);
            jumpCooldownTimer = new CountdownTimer(jumpCooldown);

            jumpTimer.OnTimerStart += () => jumpVelocity = jumpForce;
            jumpTimer.OnTimerStop += () => jumpCooldownTimer.Start();

            attackTimer = new CountdownTimer(attackCooldown);
            


            timers = new List<Timer>(3) { jumpTimer, jumpCooldownTimer, attackTimer };
        }

        void Start()
        {
            inputReader.EnablePlayerActions();
            weaponManager.ChangeWeapon(WeaponType.Sword);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            movement = new Vector3(inputReader.Direction.x, 0f, inputReader.Direction.y);
            
            stateMachine.Update();
            
            HandleTimers();
            UpdateAnimator();
        }

        void HandleTimers()
        {
            foreach (var timer in timers)
            {
                timer.Tick(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        private void UpdateAnimator()
        {
            animator.SetFloat(Speed, currentSpeed);
            
            animator.SetFloat("Velocity X", inputReader.Direction.x*currentSpeed);
            animator.SetFloat("Velocity Z", inputReader.Direction.y*currentSpeed);
        }

        private void OnEnable()
        {
            inputReader.Jump += OnJump;
            inputReader.Attack += OnAttack;

        }

        private void OnDisable()
        {
            inputReader.Jump -= OnJump;
            inputReader.Attack -= OnAttack;
        }

        void OnAttack()
        {
            if (!attackTimer.IsRunning)
            {
                attackTimer.Start();
            }
        }

        public void Hit()
        {
            Vector3 attackPos = transform.position + transform.forward;
            Collider[] hitEnemies = Physics.OverlapSphere(attackPos, attackDistance);
            
            foreach (var hitEnemy in hitEnemies)
            {
                if (hitEnemy.CompareTag("Enemy"))
                {
                    // 적 데미지 
                    hitEnemy.GetComponent<Health>().TakeDamage(attackDamage);
                }
            }
        }
        
        public void FootL()
        {
            // 나중에 발소리 사운드 재생
            //Debug.Log("왼발 착지!");
        }

        public void FootR()
        {
            //Debug.Log("오른발 착지!");
        }


        void OnJump(bool performed)
        {
            //true , 점프타이머 false, 쿨다운타이머 false, 그라운드체커true
            if (performed && !jumpTimer.IsRunning && !jumpCooldownTimer.IsRunning && groundChecker.IsGrounded)
            {
                jumpTimer.Start();
            }
            else if (!performed && jumpTimer.IsRunning)
            {
                //false true
                jumpTimer.Stop();
            }
        }
        public void HandleJump()
        {
            //점프를 하고있지않고, 땅위에있는경우
            if (!jumpTimer.IsRunning && groundChecker.IsGrounded)
            {
                jumpVelocity = ZeroF;
                jumpTimer.Stop();
                return;
            }
            
            if (!jumpTimer.IsRunning)
            {
                jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            }

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
        }

        public void HandleMovement()
        {
            //mainCam.eulerAngles.y - y축 회전값(좌우회전값)
            //Vector3.up 은 Y축(위쪽)
            //AngleAxis는 특정 축을 중심으로 특정 각도만큼 회전하는 회전값 -> Y축 중심으로 mainCam y축값만큼 회전
            //Quaternion과 Vector3을 곱하면 벡터를 회전시킨결과값이 나옴
            var cameraQuat = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up);
            adjustedDirection = cameraQuat * movement;

            //프리룩 모드아닐땐 정면(카메라방향)만 보기
            if (!inputReader.IsFreeLookMode)
            {
                HandleRotation(cameraQuat);
            }
            //프리룩모드일때 가는 방향 보기
            else
            {
                if (adjustedDirection.magnitude > ZeroF)
                {
                    HandleRotation(Quaternion.LookRotation(adjustedDirection));
                }
                
            }
            
            
            if (adjustedDirection.magnitude > ZeroF)
            {
                HandleHorizontalMovement(adjustedDirection);
                
                // 앞뒤 방향 계산
                //float forwardSpeed = Vector3.Dot(adjustedDirection, transform.forward);
                //SmoothSpeed(forwardSpeed);
                
                SmoothSpeed(adjustedDirection.magnitude);
            }
            else
            {
                SmoothSpeed(ZeroF);

                rb.linearVelocity = new Vector3(ZeroF, rb.linearVelocity.y, ZeroF);
            }
            
            
        }
        private void HandleHorizontalMovement(Vector3 adjustedDirection)
        {
            Vector3 adVelocity = adjustedDirection * (moveSpeed * Time.fixedDeltaTime);
            
            rb.linearVelocity = new Vector3(adVelocity.x, rb.linearVelocity.y, adVelocity.z);
            
            // var adjustedMovement = adjustedDirection * (moveSpeed * Time.deltaTime);
            // controller.Move(adjustedMovement);
        }

        private void HandleRotation(Quaternion targetRotation)
        {
            transform.rotation = targetRotation;
            //부드러운 회전
            //Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        private void SmoothSpeed(float value)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
        }
    }
}
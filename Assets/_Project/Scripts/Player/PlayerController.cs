
using System;
using System.Collections.Generic;
using System.Numerics;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using Utilities;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace RpgPractice
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Animator animator;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private GroundChecker groundChecker;
        [SerializeField] public WeaponManager weaponManager;
        [SerializeField] private SkillSystem skillSystem;
        [SerializeField] private CameraManager cameraManager;

        
        [Header("Settings")]
        [SerializeField] private float walkSpeed = 150f;
        [SerializeField] private float runSpeed = 400f;
        [SerializeField] private float smoothTime = 0.05f;
        [SerializeField] private float speedChangeRate = 10f;
        [Range(0.0f, 0.3f)]
        public float rotationSmoothTime = 0.12f;
        

        [Header("Jump Setting")] 
        [SerializeField] float jumpForce = 10f;
        [SerializeField] float jumpDuration = 0.5f;
        [SerializeField] float jumpCooldown = 0f;
        [SerializeField] float gravityMultiplier = 3f;
        
        [Header("Attack Setting")] 
        [SerializeField] float attackCooldown = 0.5f;
        [SerializeField] float attackDistance = 3f;
        [SerializeField] float attackPower = 10f;
        
        //test
        [SerializeField] private BossAttackTelegraph telegraphTest;
        [SerializeField] private BossAttackData attackData;
        
        private StateMachine stateMachine;
        private Transform mainCam;
        
        private const float ZeroF = 0f;

        
        
        //movement
        private Vector3 movement;
        private float moveSpeed;
        private float velocity;
        private bool sprint;
        
        private float jumpVelocity;
        
        private Vector3 adjustedDirection;

        //attack
        private int attackNum = 1;
        private bool subAttacking;
        private bool mainAttacking;
        private bool isCombo;
        
        
        //timer
        private List<Timer> timers;
        private CountdownTimer jumpTimer;
        private CountdownTimer jumpCooldownTimer;
        private CountdownTimer attackLeftTimer;
        
        // Animation
        private float animationBlend;
        private float targetRotation;
        private float rotationVelocity;
        
        private float currentVelocityX;
        private float currentVelocityZ;
        private float velocityX, velocityZ; 
        
        //animator parameters
        private static readonly int Speed = Animator.StringToHash("Speed");

        void Awake()
        {
            mainCam = Camera.main.transform;

            rb.freezeRotation = true;
            
            // Inspector에서 설정하거나 코드로
            rb.interpolation = RigidbodyInterpolation.Interpolate; // 가장 중요!
            rb.linearDamping = 5f; // 저항력 추가 (0-10 사이에서 조절)
            rb.angularDamping = 5f; // 회전 저항
            
            
            SetupTimers();
            SetupStateMachine();
        }

        private void SetupStateMachine()
        {
            stateMachine = new StateMachine();

            var locomotionState = new LocomotionState(this, animator);
            var jumpState = new JumpState(this, animator);
            var attackState = new AttackState(this, animator);
            var subAttackState = new SubAttackState(this, animator);
            var deadState = new DeadState(this, animator);

            At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
            //At(locomotionState, attackState, new FuncPredicate(() => attackTimer.IsRunning));
            //At(attackState, locomotionState, new FuncPredicate(() => !attackTimer.IsRunning));
            
            
            Any(attackState,new FuncPredicate(() => attackLeftTimer.IsRunning));
            Any(subAttackState, new FuncPredicate(() => subAttacking));
            Any(locomotionState, new FuncPredicate(ReturnToLocomotionState));
            Any(deadState, new FuncPredicate(() => transform.GetComponentInParent<Health>().IsDead));

            
            stateMachine.SetState(locomotionState);
        }

        bool ReturnToLocomotionState()
        {
            //기본애니메이션
            return groundChecker.IsGrounded
                   && !jumpTimer.IsRunning
                   && !mainAttacking
                   && !subAttacking;
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

            attackLeftTimer = new CountdownTimer(attackCooldown);

            attackLeftTimer.OnTimerStop += () => isCombo = false;

            timers = new List<Timer>(3) { jumpTimer, jumpCooldownTimer, attackLeftTimer };
        }

        void Start()
        {
            inputReader.EnablePlayerActions();
            weaponManager.ChangeWeapon(weaponManager.CurrentWeapon);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }

        void Update()
        {
            stateMachine.Update();
            
            HandleTimers();

            if (Input.GetKey(KeyCode.E))
            {
                telegraphTest.ShowTelegraph(attackData, transform.position, transform.forward);
            }

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
            UpdateAnimator();
            movement = new Vector3(inputReader.Direction.x, 0f, inputReader.Direction.y);
        }

        private void UpdateAnimator()
        {
            // Speed 애니메이션 (전체 속도)
            //animator.SetInteger("AttackNum", attackNum);
        }
        
        private void OnEnable()
        {
            //test
            
            inputReader.Dash += OnDash;
            inputReader.Jump += OnJump;
            

        }

        private void OnDisable()
        {
            //test
            
            inputReader.Dash -= OnDash;
            inputReader.Jump -= OnJump;
        }

        void OnDash(bool performed)
        {
            sprint = performed;
            animator.SetBool("sprint", sprint);
        }
        
        void OnSubAttack(bool value)
        {
            
            if (!subAttacking)
            {
                bool canSubAttack = transform.GetComponentInParent<Mana>().UseMana(20);
                if (!canSubAttack) return;
                attackLeftTimer.Stop();
                subAttacking = true;
                isCombo = false;
                
            }
        }

        void OnAttack(bool value)
        {
            if (!attackLeftTimer.IsRunning)
            {
                attackLeftTimer.Start();
                subAttacking = false;
                isCombo = false;
                animator.SetBool("LeftCombo", isCombo);
                
            }
            else if(!isCombo)
            {
                isCombo = true;
                animator.SetBool("LeftCombo", isCombo);
                //attackLeftTimer.Reset();
            }
        }

        public void Dead()
        {
            //사망 처리
            
        }

        

        public void Hit()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // if (stateInfo.IsName("Attack1"))
            // {
            //     skillSystem.UseSkill(SkillType.LeftClick, transform.position, transform.forward,transform.parent.gameObject ,attackPower);
            //     attackNum = 1;
            // }
            // else if (stateInfo.IsName("Attack2"))
            // {
            //     skillSystem.UseSkill(SkillType.Skill1, transform.position, transform.forward,transform.parent.gameObject, attackPower);
            //     attackNum = 2;
            // }
            // else if (stateInfo.IsName("SubAttack"))
            // {
            //     skillSystem.UseSkill(SkillType.RightClick, transform.position, transform.forward, transform.parent.gameObject,attackPower);
            //     attackNum = 3;
            // }
            
        }

        public void AttackEnd()
        {
            if (!isCombo || attackNum == 2)
            {
                mainAttacking = false;
                subAttacking = false;
                attackNum = 0;
                attackLeftTimer.Stop();
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
            
            // 목표 속도 지정
            float targetSpeed = sprint ? runSpeed : walkSpeed;
            
            // 입력값이 없을때 타겟속도 0으로 지정
            if (inputReader.Direction == Vector3.zero) targetSpeed = 0.0f;
            
            // y값(위아래) 제외 현재 속도
            float currentHorizontalSpeed = new Vector3(rb.linearVelocity.x, 0.0f, rb.linearVelocity.z).magnitude;
            
            // 기준값의 오차범위
            float speedOffset = 0.1f;
            
            //입력값의 속도
            float inputMagnitude = inputReader.Direction.magnitude;
            
            
            // 오차범위내 목표 속도로 가속 또는 감속, 오차범위 안이면 타겟속도 유지
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // Mathf.Lerp(시작값, 목표값, 보간 비율(0~1사이의 값)) 두값을 부드럽게 보간
                // Lerp의 T는 클램프되므로 속도를 클램프할 필요가 없음
                moveSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * speedChangeRate);
            
                // 속도를 소수점 3자리까지 반올림 , 부동소수점 이슈
                moveSpeed = Mathf.Round(moveSpeed * 1000f) / 1000f;
            }
            else
            {
                moveSpeed = targetSpeed;
            }
            
            // 입력 방향 정규화
            Vector3 inputDirection = new Vector3(inputReader.Direction.x, 0.0f, inputReader.Direction.y).normalized;
            
            // 참고: Vector2의 != 연산자는 근사치를 사용하므로 부동소수점 오차에 취약하지 않고, magnitude보다 저렴함
            // 이동 입력이 있을 때 플레이어가 움직이는 동안 플레이어 회전
            if (!cameraManager.fixedCameraMode && inputReader.Direction != Vector3.zero)
            {
                //Atan2를 사용해 입력방향을 각도로 변환하고 카메라 회전을 더함
                //카메라 기준 상대적인 방향으로 회전하기 위해 카메라의 Y축 회전값 추가
                targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  mainCam.transform.eulerAngles.y;
                
                // SmoothDampAngle을 사용해 현재 회전에서 목표 회전으로 부드럽게 회전
                // _rotationVelocity는 회전 속도를 추적하는 참조 변수
                
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                    rotationSmoothTime);
            
                // 카메라 위치에 상대적인 입력 방향을 향하도록 회전
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection;
            if (cameraManager.fixedCameraMode)
            {
                //targetDirection = transform.parent.TransformDirection(inputDirection);
                
                // 카메라 방향을 직접 사용 (부모 회전 무시)
                Vector3 cameraForward = cameraManager.GetFollowRootTransform().forward;
                Vector3 cameraRight = cameraManager.GetFollowRootTransform().right;
                cameraForward.y = 0;
                cameraRight.y = 0;
    
                targetDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;
            }
            else
            {
                targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
            }
            
            // 플레이어 이동
            // 목표 이동 벡터
            Vector3 horizontalMovement = targetDirection.normalized * moveSpeed;
            // 현재 리지드바디의 수평(y축 제외) 속도
            Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            // 목표속도와 현재속도의 차이 = 가해야 할 힘의 벡터
            Vector3 velocityChange = horizontalMovement - currentHorizontal;
            

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
            
            // 애니메이션 전환을 부드럽게 하기 위한 블렌드 값 계산
            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
            // 매우 작은 값은 0으로 처리하여 애니메이션 떨림 방지
            if (animationBlend < 0.01f) animationBlend = 0f;
            
            
            if (cameraManager.fixedCameraMode)
            {
                // 고정모드일 경우 wasd + 캐릭터방향 방향보정 해야됨
                // 아니다 캐릭터는 항상 정면을 보고있으니 w누르면 앞으로가는거고 s누르면 뒤로가는거임
                // 근대 이러면 0되는순간 바로 애니메이션이 멈춤
                currentVelocityX =
                    Mathf.SmoothDamp(currentVelocityX, inputReader.Direction.x, ref velocityX, smoothTime);
                currentVelocityZ =
                    Mathf.SmoothDamp(currentVelocityZ, inputReader.Direction.y, ref velocityZ, smoothTime);
                if (currentVelocityX * currentVelocityX < 0.0001f) currentVelocityX = 0;
                if (currentVelocityZ * currentVelocityZ < 0.0001f) currentVelocityZ = 0;
                animator.SetFloat("strafe", currentVelocityX);
                animator.SetFloat("foward", currentVelocityZ);
                
                
            }
            else
            {
                // 자유모드일경우 캐릭터가 가는곳이 곧 정면임
                // 뒷걸음 x 옆걸음 x -> 스피드값 그냥 포워드에 넣으면 됨
                animator.SetFloat("foward", targetSpeed); 
            }
            
            
            //foward, straft
            
            
            
            
            // 캐릭터를 사용하는 경우 애니메이터 업데이트
            // if (animator)
            // {
            //     // _animator.SetFloat(_animIDSpeed, _animationBlend);
            
            //     // _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            //     
            //     animator.SetFloat("Velocity X", currentVelocityX);
            //     animator.SetFloat("Velocity Z", currentVelocityZ);
            // }
        }
    }
}
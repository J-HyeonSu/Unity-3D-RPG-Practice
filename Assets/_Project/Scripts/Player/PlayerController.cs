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
        [SerializeField] private CinemachineVirtualCamera cineVCam;
        [SerializeField] private GameObject followCameraRoot;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private GroundChecker groundChecker;
        [SerializeField] public WeaponManager weaponManager;
        [SerializeField] private SkillSystem skillSystem;
        
        
        [Header("Settings")]
        [SerializeField] private float moveSpeed;
        [SerializeField] private float walkSpeed = 150f;
        [SerializeField] private float runSpeed = 400f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float smoothTime = 0.05f;

        [Header("Jump Setting")] 
        [SerializeField] float jumpForce = 10f;
        [SerializeField] float jumpDuration = 0.5f;
        [SerializeField] float jumpCooldown = 0f;
        [SerializeField] float gravityMultiplier = 3f;
        
        [Header("Attack Setting")] 
        [SerializeField] float attackCooldown = 0.5f;
        [SerializeField] float attackDistance = 3f;
        [SerializeField] float attackPower = 10f;

        [Header("Cinemachine")]
        private const float angleThreshold = 0.5f; // 0.5도 이상 변할 때만 적용
        
        
        private StateMachine stateMachine;
        
        
        private const float ZeroF = 0f;

        private Transform mainCam;
        
        //movement
        private Vector3 movement;
        private float currentSpeed;
        private float currentVelocityX;
        private float currentVelocityZ;
        private float velocity;
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
        
        //test
        private float smoothedCameraY;
        private float cameraYVelocity;
        private bool canAttack = true;
        
        // cinemachin camera
        private float TopClamp = 90f;
        private float BottomClamp = -40f;
        private float cinemachineTargetYaw;
        private float cinemachineTargetPitch;
        private bool fixedCameraMode = true;
        
        
        //animator parameters
        private static readonly int Speed = Animator.StringToHash("Speed");
        
        

        void Awake()
        {
            mainCam = Camera.main.transform;
            cineVCam.Follow = followCameraRoot.transform;
            cineVCam.LookAt = followCameraRoot.transform;
            cineVCam.OnTargetObjectWarped(
                followCameraRoot.transform,
                followCameraRoot.transform.position - cineVCam.transform.position - Vector3.forward);

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
            var subAttackState = new SubAttackState(this, animator);

            At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
            //At(locomotionState, attackState, new FuncPredicate(() => attackTimer.IsRunning));
            //At(attackState, locomotionState, new FuncPredicate(() => !attackTimer.IsRunning));
            
            
            Any(attackState,new FuncPredicate(() => attackLeftTimer.IsRunning));
            Any(subAttackState, new FuncPredicate(() => subAttacking));
            Any(locomotionState, new FuncPredicate(ReturnToLocomotionState));

            
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
            Cursor.visible = false;
        }

        void Update()
        {
            movement = new Vector3(inputReader.Direction.x, 0f, inputReader.Direction.y);
            
            stateMachine.Update();
            
            HandleTimers();
            
            //test code inputreader 사용해야함
            HandleCameraToggle();

            // if (Input.GetKeyDown(KeyCode.F1))
            // {
            //     var projectile = GameManager.instance.poolManager.Get(1);
            //     
            // }
            // if (Input.GetKeyDown(KeyCode.F2))
            // {
            //     var projectile = GameManager.instance.poolManager.Get(2);
            // }

        }

        private void HandleCameraToggle()
        {
            if (Input.GetMouseButtonDown(2))
            {
                fixedCameraMode = !fixedCameraMode;
                if (fixedCameraMode)
                {
                    //고정시점으로 변환일때
                    
                    //모델rotation을 부모오브젝트와 동기화 후 초기화
                    transform.parent.rotation = transform.rotation;
                    transform.localRotation = Quaternion.identity;
                    
                    followCameraRoot.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0, 0);
                    
                }
                else
                {
                    // 자유시점으로 변환할때
    
                    // 현재 followCameraRoot의 월드 회전값으로 동기화
                    Vector3 currentCameraEuler = followCameraRoot.transform.eulerAngles;
                    cinemachineTargetYaw = currentCameraEuler.y;
                    cinemachineTargetPitch = currentCameraEuler.x;
    
                    // 360도 처리
                    if (cinemachineTargetPitch > 180f)
                        cinemachineTargetPitch -= 360f;
    
                    // followCameraRoot를 월드 좌표 기준으로 설정
                    followCameraRoot.transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0);
                    
                }
                
                
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
        }

        private void UpdateAnimator()
        {
            animator.SetFloat(Speed, currentSpeed);
            
            // locomotion 캐릭터 이동 관련
            // 목표 속도 계산
            float targetVelocityX = inputReader.Direction.x * currentSpeed;
            float targetVelocityZ = inputReader.Direction.y * currentSpeed;

            currentVelocityX = SmoothSpeed(currentVelocityX, targetVelocityX);
            currentVelocityZ = SmoothSpeed(currentVelocityZ, targetVelocityZ);

            // currentVelocityX = inputReader.Direction.x * currentSpeed;
            // currentVelocityZ = inputReader.Direction.y * currentSpeed;
            
            
            animator.SetFloat("Velocity X", currentVelocityX);
            animator.SetFloat("Velocity Z", currentVelocityZ);
            animator.SetInteger("AttackNum", attackNum);
        }
        
        

        private void OnEnable()
        {
            //test
            inputReader.Dash += OnDash;
            inputReader.Look += OnLook;
            
            inputReader.Jump += OnJump;
            inputReader.Attack += OnAttack;
            inputReader.SubAttack += OnSubAttack;

        }

        private void OnDisable()
        {
            //test
            inputReader.Dash -= OnDash;
            inputReader.Look -= OnLook;
            
            inputReader.Jump -= OnJump;
            inputReader.Attack -= OnAttack;
            inputReader.SubAttack -= OnSubAttack;
        }

        void OnDash(bool performed)
        {
            moveSpeed = performed ? runSpeed : walkSpeed;
        }
        
        void OnLook(Vector2 cameraMovement, bool isDeviceMouse)
        {
    
            if (cameraMovement.magnitude >= 0.01f)
            {
                float deviceMultiplier = isDeviceMouse ? Time.fixedDeltaTime : Time.deltaTime;
                
                if (fixedCameraMode)
                {
                    // 고정 카메라
                    // 캐릭터 Y축 회전 (마우스 X)
                    transform.parent.Rotate(0, cameraMovement.x * deviceMultiplier, 0);

                    // 카메라 상하 회전 (마우스 Y)
                    cinemachineTargetPitch += -cameraMovement.y * deviceMultiplier;
                    cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, -30, 70);
        
                    //followCameraRoot.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0, 0);
                    followCameraRoot.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0, 0);
                }
                else
                {
                    // 자유 카메라 모드
                    cinemachineTargetYaw += cameraMovement.x * deviceMultiplier;
                    cinemachineTargetPitch += -cameraMovement.y * deviceMultiplier;
                
                    cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
                    cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);
                
                    followCameraRoot.transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0.0f);
                }
                
            }
        }
        
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
        

        void OnSubAttack()
        {
            if (!subAttacking)
            {
                attackLeftTimer.Stop();
                subAttacking = true;
                isCombo = false;
                
            }
        }

        void OnAttack()
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

        

        public void Hit()
        {
            
            canAttack = true;
            
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            

            if (stateInfo.IsName("Attack1"))
            {
                skillSystem.UseSkill(SkillType.Attack1, transform.position, transform.forward, attackPower);
                attackNum = 1;
            }
            else if (stateInfo.IsName("Attack2"))
            {
                skillSystem.UseSkill(SkillType.Attack2, transform.position, transform.forward, attackPower);
                attackNum = 2;
            }
            else if (stateInfo.IsName("SubAttack"))
            {
                skillSystem.UseSkill(SkillType.SubAttack, transform.position, transform.forward, attackPower);
                attackNum = 3;
            }

            // switch (attackNum)
            // {
            //     case 1:
            //         AttackEvent(attackNum, 5, 30, damage);
            //         break;
            //     case 2:
            //         damage = attackDamage * 1.2f;
            //         AttackEvent(attackNum, 7, 10, damage);
            //         break;
            //     case 3:
            //         damage = attackDamage * 1.5f;
            //         AttackEvent(attackNum, 7, 10, damage);
            //         break;
            //         
            // }

            // Vector3 attackPos = transform.position + transform.forward;
            // Collider[] hitEnemies = Physics.OverlapSphere(attackPos, attackDistance);

            //
            //
            // foreach (var hitEnemy in hitEnemies)
            // {
            //     if (hitEnemy.CompareTag("Enemy"))
            //     {
            //         if (stateInfo.IsName("Attack1"))
            //         {
            //             damage = attackDamage;
            //         }
            //         else if (stateInfo.IsName("Attack2"))
            //         {
            //             damage = attackDamage*1.2f;
            //         }
            //         else if (stateInfo.IsName("SubAttack"))
            //         {
            //             damage = attackDamage*1.5f;
            //         }
            //         // 적 데미지 
            //         hitEnemy.GetComponent<Health>().TakeDamage(damage);
            //         
            //         
            //     }
            // }
        }

        public void AttackEnd()
        {
            //AttackEnd가 애니메이션도중 실행이 안될때가있음
            //공격1이 끝난후 AttackEnd가 없어서 타이머대기시간이 있음.
            //공격1에 조건을 걸면 Attack2가 실행안됨

            //이제 2번째공격때 attack2 지연시간이 있음
            
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
            //mainCam.eulerAngles.y - y축 회전값(좌우회전값)
            //Vector3.up 은 Y축(위쪽)
            //AngleAxis는 특정 축을 중심으로 특정 각도만큼 회전하는 회전값 -> Y축 중심으로 mainCam y축값만큼 회전
            //Quaternion과 Vector3을 곱하면 벡터를 회전시킨결과값이 나옴
            //var cameraQuat = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up);
            
            
            //메인카메라의 회전값
            float cameraY = mainCam.eulerAngles.y;
            var cameraQuat = Quaternion.AngleAxis(cameraY, Vector3.up);
            
            // 입력된 이동방향을 카메라회전에 맞춰 보정
            adjustedDirection = cameraQuat * movement;

            float adjMagnitude = adjustedDirection.magnitude;
            bool hasMovement = adjMagnitude > ZeroF;
            
            if (hasMovement)
            {
                HandleHorizontalMovement(adjustedDirection);
                
                //자유카메라일때 이동방향으로 캐릭터 회전
                if (!fixedCameraMode)
                {
                    // 캐릭터 회전
                    //transform.rotation = Quaternion.LookRotation(adjustedDirection);
                    Quaternion targetRotation = Quaternion.LookRotation(adjustedDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                }
                
                // 애니메이션용 속도 처리
                currentSpeed = SmoothSpeed(currentSpeed, adjustedDirection.magnitude);
            }
            else
            {
                // 이동입력없으면 정지, Y축은 중력/점프때문에 유지
                rb.linearVelocity = new Vector3(ZeroF, rb.linearVelocity.y, ZeroF);
                // 애니메이션용 속도 처리
                currentSpeed = SmoothSpeed(currentSpeed, ZeroF);
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
        
        private float SmoothSpeed(float speed, float value)
        {
            return Mathf.SmoothDamp(speed, value, ref velocity, smoothTime);
        }
    }
}
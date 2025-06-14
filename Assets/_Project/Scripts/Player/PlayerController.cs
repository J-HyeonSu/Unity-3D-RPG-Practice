using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
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
        [SerializeField] private float fixedRotateSpeed = 10f;

        [Header("Jump Setting")] 
        [SerializeField] float jumpForce = 10f;
        [SerializeField] float jumpDuration = 0.5f;
        [SerializeField] float jumpCooldown = 0f;
        [SerializeField] float gravityMultiplier = 3f;
        
        [Header("Attack Setting")] 
        [SerializeField] float attackCooldown = 0.5f;
        [SerializeField] float attackDistance = 3f;
        [SerializeField] float attackPower = 10f;
        
        private StateMachine stateMachine;
        
        
        private const float ZeroF = 0f;

        private Transform mainCam;
        
        //movement
        private Vector3 movement;
        private float currentSpeed;
        private float velocity;
        private float jumpVelocity;
        private bool sprint;
        
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
        float SpeedChangeRate = 10f;
        private float _animationBlend;
        private float _targetRotation;
        private float _rotationVelocity;
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        
        private float targetVelocityX;
        private float targetVelocityZ;
        private float currentVelocityX;
        private float currentVelocityZ;
        private float velocityX;
        private float velocityZ;
        
        private float rotationCameraVelocity;
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
            movement = new Vector3(inputReader.Direction.x, 0f, inputReader.Direction.y);
        }

        private void UpdateAnimator()
        {
            // Speed 애니메이션 (전체 속도)
            animator.SetFloat(Speed, _animationBlend); // currentSpeed 대신 _animationBlend 사용
    
            // Velocity X, Z 계산 (카메라 기준으로 변환된 입력값)
            float cameraY = mainCam.eulerAngles.y;
            var cameraQuat = Quaternion.AngleAxis(cameraY, Vector3.up);
            Vector3 cameraRelativeMovement = cameraQuat * movement;
    
            // 목표 속도 계산
            targetVelocityX = cameraRelativeMovement.x;
            targetVelocityZ = cameraRelativeMovement.z;
    
            // 부드러운 블렌딩
            currentVelocityX = Mathf.SmoothDamp(currentVelocityX, targetVelocityX, ref velocityX, smoothTime);
            currentVelocityZ = Mathf.SmoothDamp(currentVelocityZ, targetVelocityZ, ref velocityZ, smoothTime);
    
            // 애니메이터에 전달
            //animator.SetFloat("Velocity X", currentVelocityX);
            //animator.SetFloat("Velocity Z", currentVelocityZ);
            
            
            // animator.SetFloat("Velocity X", currentVelocityX);
            // animator.SetFloat("Velocity Z", currentVelocityZ);
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
            sprint = performed;
        }
                
        // inputreader collback 함수
        void OnLook(Vector2 cameraMovement, bool isDeviceMouse)
        {
            
            if (cameraMovement.magnitude >= 0.01f)
            {
                float deviceMultiplier = isDeviceMouse ? Time.fixedDeltaTime : Time.deltaTime;
                
                if (fixedCameraMode)
                {
                    // 고정 카메라 모드
                    // 캐릭터 Y축 회전 (마우스 X)
                    
                    //카메라 회전하는만큼 캐릭터도 회전
                    
                    //카메라 회전값
                    float rotateY = cameraMovement.x * deviceMultiplier * fixedRotateSpeed + transform.parent.eulerAngles.y;
                    //현재값
                    
                    //_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCam.transform.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(transform.parent.eulerAngles.y, rotateY, ref rotationCameraVelocity,
                        RotationSmoothTime);
            
                    // 카메라 위치에 상대적인 입력 방향을 향하도록 회전
                    transform.parent.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                    
                    //렉걸리던값
                    //transform.parent.Rotate(0, cameraMovement.x * deviceMultiplier, 0);

                    // 카메라 상하 회전 (마우스 Y)
                    cinemachineTargetPitch += -cameraMovement.y * deviceMultiplier;
                    cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);
        
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
                skillSystem.UseSkill(SkillType.Attack1, transform.position, transform.forward,transform.parent.gameObject ,attackPower);
                attackNum = 1;
            }
            else if (stateInfo.IsName("Attack2"))
            {
                skillSystem.UseSkill(SkillType.Attack2, transform.position, transform.forward,transform.parent.gameObject, attackPower);
                attackNum = 2;
            }
            else if (stateInfo.IsName("SubAttack"))
            {
                skillSystem.UseSkill(SkillType.SubAttack, transform.position, transform.forward, transform.parent.gameObject,attackPower);
                attackNum = 3;
            }
            
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
                // 참고: Lerp의 T는 클램프되므로 속도를 클램프할 필요가 없음
                moveSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);
            
                // 속도를 소수점 3자리까지 반올림 , 부동소수점 이슈
                moveSpeed = Mathf.Round(moveSpeed * 1000f) / 1000f;
            }
            else
            {
                moveSpeed = targetSpeed;
            }
            
            // 애니메이션 전환을 부드럽게 하기 위한 블렌드 값 계산
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            // 매우 작은 값은 0으로 처리하여 애니메이션 떨림 방지
            if (_animationBlend < 0.01f) _animationBlend = 0f;
            
            // 입력 방향 정규화
            Vector3 inputDirection = new Vector3(inputReader.Direction.x, 0.0f, inputReader.Direction.y).normalized;
            
            // 참고: Vector2의 != 연산자는 근사치를 사용하므로 부동소수점 오차에 취약하지 않고, magnitude보다 저렴함
            // 이동 입력이 있을 때 플레이어가 움직이는 동안 플레이어 회전
            if (!fixedCameraMode && inputReader.Direction != Vector3.zero)
            {
                //Atan2를 사용해 입력방향을 각도로 변환하고 카메라 회전을 더함
                //카메라 기준 상대적인 방향으로 회전하기 위해 카메라의 Y축 회전값 추가
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  mainCam.transform.eulerAngles.y;
                
                // SmoothDampAngle을 사용해 현재 회전에서 목표 회전으로 부드럽게 회전
                // _rotationVelocity는 회전 속도를 추적하는 참조 변수
                
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);
            
                // 카메라 위치에 상대적인 입력 방향을 향하도록 회전
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection;
            if (fixedCameraMode)
            {
                targetDirection = transform.parent.TransformDirection(inputDirection);
            }
            else
            {
                targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            }
             
            
            
            // 플레이어 이동
            Vector3 horizontalMovement = targetDirection.normalized * moveSpeed;
            Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 velocityChange = horizontalMovement - currentHorizontal;

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
            
            
            
            
            
            // _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
            //                  new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            
            // 캐릭터를 사용하는 경우 애니메이터 업데이트
            // if (animator)
            // {
            //     // _animator.SetFloat(_animIDSpeed, _animationBlend);
            
            //     // _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            //     
            //     animator.SetFloat("Velocity X", currentVelocityX);
            //     animator.SetFloat("Velocity Z", currentVelocityZ);
            // }
            
            
            
            
            
            
            
            
            return;
            
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
                currentSpeed = Mathf.SmoothDamp(currentSpeed, adjMagnitude, ref velocity, smoothTime);
            }
            else
            {
                // 이동입력없으면 정지, Y축은 중력/점프때문에 유지
                rb.linearVelocity = new Vector3(ZeroF, rb.linearVelocity.y, ZeroF);
                // 애니메이션용 속도 처리
                currentSpeed = Mathf.SmoothDamp(currentSpeed, ZeroF, ref velocity, smoothTime);
            }
            
            
            
        }
        private void HandleHorizontalMovement(Vector3 moveDirection)
        {
            Vector3 targetVelocity = moveDirection* moveSpeed;
            Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 velocityChange = targetVelocity - currentHorizontal;
            
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
            
            // Vector3 adVelocity = moveDirection * (moveSpeed * Time.fixedDeltaTime);
            // rb.linearVelocity = new Vector3(adVelocity.x, rb.linearVelocity.y, adVelocity.z);
            
            
        }
        
        private float SmoothSpeed(float speed, float value)
        {
            return Mathf.SmoothDamp(speed, value, ref velocity, smoothTime);
        }
    }
}
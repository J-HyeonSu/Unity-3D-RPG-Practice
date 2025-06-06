using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.XR;

namespace RpgPractice
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private CharacterController controller;
        [SerializeField] private Animator animator;
        [SerializeField] private CinemachineFreeLook freeLookVCam;
        [SerializeField] private InputReader inputReader;
        
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float smoothTime = 0.2f;
        private const float ZeroF = 0f;

        private Transform mainCam;
        private float currentSpeed;
        private float velocity;
        
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
        }

        void Start()
        {
            inputReader.EnablePlayerActions();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            HandleMovement();
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            animator.SetFloat(Speed, currentSpeed);
        }


        public void HandleJump()
        {
            
        }

        public void HandleMovement()
        {
            
            //입력값의 방향값
            var movementDirection = new Vector3(inputReader.Direction.x, 0, inputReader.Direction.y).normalized;
            //mainCam.eulerAngles.y - y축 회전값(좌우회전값)
            //Vector3.up 은 Y축(위쪽)
            //AngleAxis는 특정 축을 중심으로 특정 각도만큼 회전하는 회전값 -> Y축 중심으로 mainCam y축값만큼 회전
            //Quaternion과 Vector3을 곱하면 벡터를 회전시킨결과값이 나옴
            
            var cameraQuat = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up);
            var adjustedDirection = cameraQuat * movementDirection;

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
                HandleCharacterController(adjustedDirection);
                SmoothSpeed(adjustedDirection.magnitude);
            }
            else
            {
                SmoothSpeed(ZeroF);
            }
            
            
        }
        private void HandleCharacterController(Vector3 adjustedDirection)
        {
            var adjustedMovement = adjustedDirection * (moveSpeed * Time.deltaTime);
            controller.Move(adjustedMovement);
        }

        private void HandleRotation(Quaternion targetRotation)
        {
            //부드러운 회전
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        private void SmoothSpeed(float value)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
        }
    }
}
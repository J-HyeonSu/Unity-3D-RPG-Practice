using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

namespace RpgPractice
{
    public class CameraManager : MonoBehaviour
    {
        
        [SerializeField] private InputReader input;
        [SerializeField, Range(0.5f, 3f)] private float speedMultiplier;
        
        [Header("Camera Settings")]
        [SerializeField] private float TopClamp = 90f;
        [SerializeField] private float BottomClamp = -40f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private CinemachineVirtualCamera cineVCam;
        [SerializeField] private GameObject followCameraRoot;
        
        [Header("Camera Sensitivity")]
        [SerializeField] private float sensitivity = 1.0f;
        
        [Header("Camera Zoom")]
        [SerializeField] private float zoomSpeed = 0.8f;
        [SerializeField] private float zoomMinClamp = 3.0f;
        [SerializeField] private float zoomMaxClamp = 15.0f;


        // Camera
        private float cinemachineTargetYaw;
        private float cinemachineTargetPitch;
        public bool fixedCameraMode { get; private set; } = true;
        private Vector3 inputCameraMovement;
        private bool isDeviceMouse;

        //test
        private Cinemachine3rdPersonFollow thirdPersonFollow;
        private void Start()
        {
            cineVCam.Follow = followCameraRoot.transform;
            cineVCam.LookAt = followCameraRoot.transform;
            cineVCam.OnTargetObjectWarped(
                followCameraRoot.transform,
                followCameraRoot.transform.position - cineVCam.transform.position - Vector3.forward);
            
            cinemachineTargetYaw = followCameraRoot.transform.rotation.eulerAngles.y;
            cinemachineTargetPitch = followCameraRoot.transform.rotation.eulerAngles.x;

            thirdPersonFollow = cineVCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        }

        void OnEnable()
        {
            input.Look += OnLook;
            input.FixedCamera += SetFixedCameraMode;
            input.Scroll += InputReader_Scroll;


        }
        void OnDisable()
        {
            input.Look -= OnLook;
            input.FixedCamera -= SetFixedCameraMode;
            input.Scroll -= InputReader_Scroll;
        }

        private void InputReader_Scroll(float scrollValue)
        {
            if (thirdPersonFollow != null)
            {
                // 현재 카메라 거리 가져오기
                float currentDistance = thirdPersonFollow.CameraDistance;
            
                // 스크롤 값에 따라 거리 조절
                float newDistance = currentDistance - (scrollValue * zoomSpeed);
            
                // 최소/최대 거리 제한
                newDistance = Mathf.Clamp(newDistance, zoomMinClamp, zoomMaxClamp);
            
                // 새로운 거리 적용
                thirdPersonFollow.CameraDistance = newDistance;
            }
        }

        private void LateUpdate()
        {
            HandleCameraRotation();
        }

        // inputreader collback 함수
        private void OnLook(Vector2 cameraMovement, bool isMouse)
        {
            inputCameraMovement = cameraMovement;
            isDeviceMouse = isMouse;
        }
        
        void HandleCameraRotation()
        {
            if (inputCameraMovement != Vector3.zero)
            {
                float deviceMultiplier = isDeviceMouse ? 1.0f : Time.deltaTime;
                
                // 입력값을 카메라 회전각에 누적 (X축은 좌우, Y축은 상하 - 반전)
                cinemachineTargetYaw += inputCameraMovement.x * deviceMultiplier * sensitivity;
                cinemachineTargetPitch += -inputCameraMovement.y * deviceMultiplier * sensitivity;
                
            }
            
            // 카메라 회전각 제한 (Yaw는 무제한, Pitch는 위아래 제한)
            cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
            cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);
                
            // 계산된 회전값을 실제 카메라 루트에 적용
            followCameraRoot.transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0.0f);

            // 고정 카메라 모드일 때: 플레이어도 카메라 방향으로 회전
            if (fixedCameraMode)
            {
                // 카메라의 전방 방향을 구하고 Y축 제거 (수평 방향만)
                Vector3 cameraDirection = followCameraRoot.transform.forward;
                cameraDirection.y = 0;
                
                // 카메라 방향으로 플레이어 부모 오브젝트를 부드럽게 회전
                var targetFRotation =  Quaternion.LookRotation(cameraDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetFRotation, rotationSpeed * Time.deltaTime);
                //transform.parent.rotation = Quaternion.Slerp(transform.parent.rotation, targetFRotation, rotationSpeed * Time.deltaTime);
                
            }
        }

        void SetFixedCameraMode(bool value)
        {
            fixedCameraMode = !fixedCameraMode;
            
            if (fixedCameraMode)
            {
                //고정시점으로 변환일때
                
                //모델rotation을 부모오브젝트와 동기화 후 초기화
                transform.parent.rotation = transform.rotation;
                transform.localRotation = Quaternion.identity;
                
                //followCameraRoot.transform.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0, 0);
                
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

        public Transform GetFollowRootTransform()
        {
            return followCameraRoot.transform;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
        
    }
}
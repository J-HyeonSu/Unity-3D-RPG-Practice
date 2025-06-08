using System.Collections;
using Cinemachine;
using UnityEngine;

namespace RpgPractice
{
    public class CameraManager : MonoBehaviour
    {
        
        [SerializeField] private InputReader input;
        //[SerializeField] private CinemachineFreeLook freeLookVCam;

        [SerializeField, Range(0.5f, 3f)] private float speedMultiplier;

        public GameObject CinemachineCameraTarget;
        private float cinemachineTargetYaw;
        private float cinemachineTargetPitch;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        

        private bool isButtonPressed;
        private bool cameraMovementLock;

        void OnEnable()
        {
            input.Look += OnLook;
            input.EnableMouseControlCamera += OnEnableMouseControlCamera;
            input.DisableMouseControlCamera += OnDisableMouseControlCamera;
            
        }
        void OnDisable()
        {
            input.Look -= OnLook;
            input.EnableMouseControlCamera -= OnEnableMouseControlCamera;
            input.DisableMouseControlCamera -= OnDisableMouseControlCamera;
        }

        
        void OnLook(Vector2 cameraMovement, bool isDeviceMouse)
        {
            if (cameraMovementLock) return;
            //if (isDeviceMouse && !isButtonPressed) return;

            float deviceMultiplier = isDeviceMouse ? Time.fixedDeltaTime : Time.deltaTime;
            // freeLookVCam.m_XAxis.m_InputAxisValue = cameraMovement.x * speedMultiplier * deviceMultiplier;
            // freeLookVCam.m_YAxis.m_InputAxisValue = cameraMovement.y * speedMultiplier * deviceMultiplier;

            cinemachineTargetPitch = Mathf.Clamp(cinemachineTargetPitch, BottomClamp, TopClamp);
            
            cinemachineTargetYaw += cameraMovement.x * deviceMultiplier;
            cinemachineTargetPitch += -cameraMovement.y * deviceMultiplier;
            
            cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
            cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, BottomClamp, TopClamp);

            
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0.0f);
        }

        void OnEnableMouseControlCamera()
        {
            isButtonPressed = true;

            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;

            StartCoroutine(DisableMouseForFrame());
        }
        IEnumerator DisableMouseForFrame()
        {
            cameraMovementLock = true;
            yield return new WaitForEndOfFrame();
            cameraMovementLock = false;
        }

        void OnDisableMouseControlCamera()
        {
            isButtonPressed = false;

            // Cursor.lockState = CursorLockMode.None;
            // Cursor.visible = true;

            // freeLookVCam.m_XAxis.m_InputAxisValue = 0f;
            // freeLookVCam.m_YAxis.m_InputAxisValue = 0f;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
        
    }
}
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace RpgPractice
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "RpgPractice/InputReader")]
    public class InputReader : ScriptableObject, RpgInputAction.IPlayerActions
    {
        public event UnityAction<Vector2> Move ;
        public event UnityAction<Vector2, bool> Look ;
        public event UnityAction<bool> Jump ;
        public event UnityAction<bool> Dash ;
        public event UnityAction<bool> LeftClick ;
        public event UnityAction<bool> RightClick ;
        public event UnityAction<bool> FixedCamera ;
        public event UnityAction<float> Scroll;
        public event UnityAction<bool> Skill1;
        public event UnityAction<bool> Skill2;
        public event UnityAction<bool> Skill3;
        public event UnityAction<bool> Skill4;

        private RpgInputAction inputAction;
        
        public Vector3 Direction => inputAction.Player.Move.ReadValue<Vector2>();

        private void OnEnable()
        {
            if (inputAction == null)
            {
                inputAction = new RpgInputAction();
                inputAction.Player.SetCallbacks(this);
            }
        }
        public void EnablePlayerActions()
        {
            inputAction.Enable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Move?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Look?.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }
        bool IsDeviceMouse(InputAction.CallbackContext context) => context.control.device.name == "Mouse";
        
        public void OnCameraZoom(InputAction.CallbackContext context)
        {
            Scroll?.Invoke(context.ReadValue<float>());
        }
        
        private void HandleKeyInput(InputAction.CallbackContext context, UnityAction<bool> action)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    action?.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    action?.Invoke(false);
                    break;
            }
        }

        public void OnSkill1(InputAction.CallbackContext context)=> HandleKeyInput(context, Skill1);
        public void OnSkill2(InputAction.CallbackContext context)=> HandleKeyInput(context, Skill2);
        public void OnSkill3(InputAction.CallbackContext context)=> HandleKeyInput(context, Skill3);
        public void OnSkill4(InputAction.CallbackContext context)=> HandleKeyInput(context, Skill4);
        public void OnLeftClick(InputAction.CallbackContext context) => HandleKeyInput(context, LeftClick);
        public void OnRightClick(InputAction.CallbackContext context) => HandleKeyInput(context, RightClick);
        public void OnFixCameraMode(InputAction.CallbackContext context) => HandleKeyInput(context, FixedCamera);
        public void OnDash(InputAction.CallbackContext context) => HandleKeyInput(context, Dash);
        public void OnJump(InputAction.CallbackContext context) => HandleKeyInput(context, Jump);

    }
}

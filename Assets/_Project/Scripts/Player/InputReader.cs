using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
namespace RpgPractice
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "RpgPractice/InputReader")]
    public class InputReader : ScriptableObject, RpgInputAction.IPlayerActions
    {
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { };
        public event UnityAction EnableMouseControlCamera = delegate { };
        public event UnityAction DisableMouseControlCamera = delegate { };
        public event UnityAction<bool> Jump = delegate { };
        public event UnityAction Attack = delegate { };

        private RpgInputAction inputAction;

        public bool IsFreeLookMode { get; private set; }
        
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
            Move.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }

        bool IsDeviceMouse(InputAction.CallbackContext context) => context.control.device.name == "Mouse";

        
        public void OnMouseControlCamera(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    IsFreeLookMode = true;
                    EnableMouseControlCamera.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    IsFreeLookMode = false;
                    DisableMouseControlCamera.Invoke();
                    break;
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Jump.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Jump.Invoke(false);
                    break;
            }
        }


        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                Attack.Invoke();                
            }
            
        }

        
    }
}

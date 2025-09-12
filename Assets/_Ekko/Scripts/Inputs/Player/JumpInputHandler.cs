using UnityEngine;
using UnityEngine.InputSystem;

namespace _Ekko.Scripts.Inputs
{
    public class JumpInputHandler : InputHandler
    {
        [SerializeField] 
        private JumpSystem jumpSystem;
        
        protected override void RegisterInputActions()
        {
            var inputActionMap = GetActionMap();
            if (inputActionMap == null)
                return;
            
            inputActionMap.Player.Jump.started += OnJumpPerformed;
            inputActionMap.Player.Jump.canceled += OnJumpCanceled;
            
            
            inputActionMap.Player.Slam.started += OnSlamStarted;
            inputActionMap.Player.Slam.canceled += OnSlamCanceled;

            inputActionMap.Player.Cushion.performed += OnCushionPerformed;
        }

        protected override void UnregisterInputActions()
        {
            var inputActionMap = GetActionMap();
            if (inputActionMap == null)
                return;
            
            inputActionMap.Player.Jump.started -= OnJumpPerformed;
            inputActionMap.Player.Jump.canceled -= OnJumpCanceled;
            
            
            inputActionMap.Player.Slam.started -= OnSlamStarted;
            inputActionMap.Player.Slam.canceled -= OnSlamCanceled;
            
            inputActionMap.Player.Cushion.performed -= OnCushionPerformed;
        }
        
        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            jumpSystem.JumpCanceled();
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            jumpSystem.JumpPerformed();
        }
        
        private void OnSlamCanceled(InputAction.CallbackContext obj)
        {
            jumpSystem.StopSlam();
        }

        private void OnSlamStarted(InputAction.CallbackContext obj)
        {
            jumpSystem.Slam();
        }
        
        private void OnCushionPerformed(InputAction.CallbackContext obj)
        {
            jumpSystem.OnCushion();
        }

    }
}
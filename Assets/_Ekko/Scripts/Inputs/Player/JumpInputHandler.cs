using _Ekko.Scripts.Gameplay.CharacterController;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Ekko.Scripts.Inputs
{
    public class JumpInputHandler : InputHandler
    {
        [SerializeField]
        private bool is3D = false;
        [SerializeField] 
        private JumpSystem jumpSystem;
        
        [SerializeField] 
        private JumpSystem3D jumpSystem3D;
        
        protected override void RegisterInputActions()
        {
            var inputActionMap = GetActionMap();
            if (inputActionMap == null)
                return;
            
            inputActionMap.Player.Jump.started += OnJumpPerformed;
            inputActionMap.Player.Jump.canceled += OnJumpCanceled;
            
            
            inputActionMap.Player.Slam.started += OnSlamStarted;
            inputActionMap.Player.Slam.canceled += OnSlamCanceled;

            if (is3D)
            {
                inputActionMap.Player.Cushion.started += OnCushionStarted;
                inputActionMap.Player.Cushion.canceled += OnCushionCanceled;
            }
            else
            {
                inputActionMap.Player.Cushion.performed += OnCushionPerformed;
            }
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
            if (is3D)
            {
                inputActionMap.Player.Cushion.started -= OnCushionStarted;
                inputActionMap.Player.Cushion.canceled -= OnCushionCanceled;
            }
            else
            {
                inputActionMap.Player.Cushion.performed -= OnCushionPerformed;
            }
        }
        
        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            if (is3D)
            {
                jumpSystem3D.OnJumpReleased();
                return;
            }
            jumpSystem.JumpCanceled();
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        { 
            if (is3D)
            {
                jumpSystem3D.OnJumpPressed();
                return;
            }
            jumpSystem.JumpPerformed();
        }
        
        private void OnSlamCanceled(InputAction.CallbackContext obj)
        {
            if (is3D)
            {
                jumpSystem3D.OnSlamReleased();
                return;
            }
            jumpSystem.StopSlam();
        }

        private void OnSlamStarted(InputAction.CallbackContext obj)
        {
            if (is3D)
            {
                jumpSystem3D.OnSlamPressed();
                return;
            }
            jumpSystem.Slam();
        }
        
        private void OnCushionPerformed(InputAction.CallbackContext obj)
        {
            jumpSystem.OnCushion();
        }
        
        private void OnCushionCanceled(InputAction.CallbackContext obj)
        {
            jumpSystem3D.OnCushionReleased();
        }

        private void OnCushionStarted(InputAction.CallbackContext obj)
        {
            jumpSystem3D.OnCushionPressed();
        }

    }
}
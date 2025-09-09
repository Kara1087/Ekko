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
            PlayerInput playerInput = GetPlayerInput();
            if (playerInput == null)
                return;
            
            playerInput.actions["Jump"].started += OnJumpPerformed;
            playerInput.actions["Jump"].canceled += OnJumpCanceled;
            
            
            playerInput.actions["Slam"].started += OnSlamStarted;
            playerInput.actions["Slam"].canceled += OnSlamCanceled;

            playerInput.actions["Cushion"].performed += OnCushionPerformed;
        }

        protected override void UnregisterInputActions()
        {
            PlayerInput playerInput = GetPlayerInput();
            if (playerInput == null)
                return;
               
            playerInput.actions["Jump"].started -= OnJumpPerformed;
            playerInput.actions["Jump"].canceled -= OnJumpCanceled;
            
            
            playerInput.actions["Slam"].started -= OnSlamStarted;
            playerInput.actions["Slam"].canceled -= OnSlamCanceled;

            playerInput.actions["Cushion"].performed -= OnCushionPerformed;
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
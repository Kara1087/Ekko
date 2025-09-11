using UnityEngine;
using UnityEngine.InputSystem;

namespace _Ekko.Scripts.Inputs
{
    public class MoveInputHandler : InputHandler
    {
       [SerializeField] private PlayerController playerController;
       private Vector2 moveInput;
       
       protected override void RegisterInputActions()
       { 
           var playerInput = GetPlayerInput();
           if (playerInput == null)
               return;
           
           playerInput.actions["Move"].performed += OnMovePerformed;
           playerInput.actions["Move"].canceled += OnMoveCanceled;
       }

       protected override void UnregisterInputActions()
       {
           PlayerInput playerInput = GetPlayerInput();
           if (playerInput == null)
               return;
           
           playerInput.actions["Move"].performed -= OnMovePerformed;
           playerInput.actions["Move"].canceled -= OnMoveCanceled;
       }
       
       // Utilisez des noms différents pour éviter les conflits potentiels
       private void OnMovePerformed(InputAction.CallbackContext context)
       {
           moveInput = context.ReadValue<Vector2>();
           playerController.SetMoveDirection(moveInput);
       }
       
       private void OnMoveCanceled(InputAction.CallbackContext context)
       {
           moveInput = Vector2.zero;
           playerController.SetMoveDirection(moveInput);
       }
    }
}
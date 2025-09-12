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
           var inputActionMap = GetActionMap();
           if (inputActionMap == null)
               return;
           
           inputActionMap.Player.Move.performed += OnMovePerformed;
           inputActionMap.Player.Move.canceled += OnMoveCanceled;
       }

       protected override void UnregisterInputActions()
       {
           var inputActionMap = GetActionMap();
           if (inputActionMap == null)
               return;
           
           inputActionMap.Player.Move.performed -= OnMovePerformed;
           inputActionMap.Player.Move.canceled -= OnMoveCanceled;
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
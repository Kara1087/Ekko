using UnityEngine.InputSystem;

namespace _Ekko.Scripts.Inputs
{
   public class PauseInputHandler : InputHandler
    {
        private InputAction pauseAction;
    
        protected override void RegisterInputActions()
        {
            var playerInput = GetPlayerInput();
            if (playerInput == null)
                return;
            
            pauseAction = playerInput.actions["Pause"]; 
            pauseAction.performed += OnPausePerformed;
        }
    
        protected override void UnregisterInputActions()
        {
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPausePerformed;
            }
        }
    
        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            if (!GameManager.Instance.IsGameOver)
            {
                GameManager.Instance.TogglePause();
            }
        }
    }
}
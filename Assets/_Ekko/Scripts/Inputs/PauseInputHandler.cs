using UnityEngine;
using UnityEngine.InputSystem;

namespace _Ekko.Scripts.Inputs
{
   public class PauseInputHandler : InputHandler
    {
    
        protected override void RegisterInputActions()
        {
            var inputActionMap = GetActionMap();
            if (inputActionMap == null)
                return;
            
            inputActionMap.Player.Pause.started += OnPauseStarted;
            inputActionMap.UI.Pause.started += OnPauseStarted;
        }
    
        protected override void UnregisterInputActions()
        {
            var inputActionMap = GetActionMap();
            if (inputActionMap == null)
                return;
        
            inputActionMap.Player.Pause.started -= OnPauseStarted;
            inputActionMap.UI.Pause.started -= OnPauseStarted;
        }
    
        private void OnPauseStarted(InputAction.CallbackContext context)
        {
            Debug.Log("PAUSE!");
            if (!GameManager.Instance.IsGameOver)
            {
                GameManager.Instance.TogglePause();
            }
        }
    }
}
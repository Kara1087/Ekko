using UnityEngine;
using UnityEngine.InputSystem;

namespace _Ekko.Scripts.Inputs
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
  
        public InputSystem_Actions InputActions { get; private set; }

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InputActions = new InputSystem_Actions();
            SwitchToGameplayControls();
        }
    
        public void SwitchToGameplayControls()
        {
            InputActions.UI.Disable();
            InputActions.Player.Enable();
        }
    
        public void SwitchToUIControls()
        {
            InputActions.Player.Disable();
            InputActions.UI.Enable();
        }
        
        private void OnDestroy()
        {
            InputActions?.Dispose();
        }
    }
}
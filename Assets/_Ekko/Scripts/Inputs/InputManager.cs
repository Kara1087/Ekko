using UnityEngine;
using UnityEngine.InputSystem;

namespace _Ekko.Scripts.Inputs
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
  
        [SerializeField] private PlayerInput playerInput;
        
        public PlayerInput CurrentPlayerInput => playerInput;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
      
            Instance = this;
        }
    
        public void SwitchToGameplayControls()
        {
            playerInput.SwitchCurrentActionMap("Gameplay");
        }
    
        public void SwitchToUIControls()
        {
            playerInput.SwitchCurrentActionMap("UI");
        }
    }
}
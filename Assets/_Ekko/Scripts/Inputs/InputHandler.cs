using UnityEngine;

namespace _Ekko.Scripts.Inputs
{
    public abstract class InputHandler : MonoBehaviour
    {
        protected virtual void Start()
        {
            if (InputManager.Instance != null && InputManager.Instance.InputActions != null)
            {
                RegisterInputActions();
            }
            else
            {
                Debug.LogError($"InputHandler in {gameObject.name} can't be Init: InputManager not find or PlayerInput null");
            }
        }
        protected virtual void OnEnable()
        {
            // Si déjà démarré, on s'assure que les actions sont enregistrées
            if (InputManager.Instance != null && InputManager.Instance.InputActions != null)
            {
                RegisterInputActions();
            }
        }
        protected virtual void OnDisable()
        {
            // Si l'InputManager existe toujours, on désenregistre nos actions
            if (InputManager.Instance != null && InputManager.Instance.InputActions != null)
            {
                UnregisterInputActions();
            }
        }
        protected abstract void RegisterInputActions();
        protected abstract void UnregisterInputActions();
        protected InputSystem_Actions GetActionMap()
        {
            if (InputManager.Instance != null)
            {
                return InputManager.Instance.InputActions;
            }
            return null;
        }
    }

}
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpReleased { get; private set; }

    public bool ControlFallPressedThisFrame { get; private set; }
    public bool DownHeld { get; private set; }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            JumpPressedThisFrame = true;

        if (context.canceled)
            JumpReleased = true;
    }

    public void OnControlFall(InputAction.CallbackContext context)
    {
        if (context.performed)
            ControlFallPressedThisFrame = true;
    }

    public void OnSlam(InputAction.CallbackContext context)
    {
        DownHeld = context.ReadValueAsButton(); // reste "true" tant que la touche est maintenue
    }

    private void LateUpdate()
    {
        // Reset à chaque frame
        JumpPressedThisFrame = false;
        JumpReleased = false;
        ControlFallPressedThisFrame = false;
        // DownHeld est géré en continu
    }

}

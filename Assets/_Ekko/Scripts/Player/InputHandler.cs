using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool JumpReleased { get; private set; }

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

    private void LateUpdate()
    {
        // Reset à chaque frame
        JumpPressedThisFrame = false;
        JumpReleased = false;
    }

}

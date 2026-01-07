using UnityEngine;

namespace _Ekko.Scripts.Gameplay.CharacterController
{
    public interface IMovementMode
    {
        Vector3 CalculateMoveDirection(Vector3 inputDirection, Transform cameraTransform);
        Quaternion CalculateRotation(Vector3 moveDirection, Transform playerTransform);
        bool AllowVerticalMovement { get; }
    }
}
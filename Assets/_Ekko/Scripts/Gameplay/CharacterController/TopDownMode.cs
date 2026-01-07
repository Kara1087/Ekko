using System;
using UnityEngine;

namespace _Ekko.Scripts.Gameplay.CharacterController
{
    [Serializable]
    public class TopDownMode : IMovementMode
    {
        public bool AllowVerticalMovement => false;

        public Vector3 CalculateMoveDirection(Vector3 inputDirection, Transform cameraTransform)
        {
            return new Vector3(inputDirection.x, 0f, inputDirection.y).normalized;
        }

        public Quaternion CalculateRotation(Vector3 moveDirection, Transform playerTransform)
        {
            if (moveDirection.sqrMagnitude < 0.01f) return playerTransform.rotation;
            return Quaternion.LookRotation(moveDirection);
        }
    }
}
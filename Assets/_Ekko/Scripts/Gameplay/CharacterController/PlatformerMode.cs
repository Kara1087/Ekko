using System;
using UnityEngine;

namespace _Ekko.Scripts.Gameplay.CharacterController
{
    [Serializable]
    public class PlatformerMode : IMovementMode
    {
        public bool AllowVerticalMovement => false;
        
        public Vector3 CalculateMoveDirection(Vector3 inputDirection, Transform cameraTransform)
        {
            //TODO maybe add modality to change axis of movement
            return new Vector3(inputDirection.x, 0f, 0f);
        }

        public Quaternion CalculateRotation(Vector3 moveDirection, Transform playerTransform)
        {
            if (moveDirection.x == 0) return playerTransform.rotation;
        
            return Quaternion.LookRotation(moveDirection.x > 0 ? Vector3.right : Vector3.left);
        }

    }
}
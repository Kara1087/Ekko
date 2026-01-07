using System;
using UnityEngine;

namespace _Ekko.Scripts.Gameplay.CharacterController
{
    [Serializable]
    public class FreeMode : IMovementMode
    {
        public bool AllowVerticalMovement => false;
        public Vector3 CalculateMoveDirection(Vector3 inputDirection, Transform cameraTransform)
        {  
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
        
            return (forward * inputDirection.y + right * inputDirection.x).normalized;
        }

        public Quaternion CalculateRotation(Vector3 moveDirection, Transform playerTransform)
        {
            if (moveDirection.sqrMagnitude < 0.01f) 
                return playerTransform.rotation;
            
            return Quaternion.LookRotation(moveDirection);
        }
    }
}
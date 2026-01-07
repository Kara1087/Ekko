using _Ekko.Scripts.Gameplay.CharacterController;
using UnityEngine;

namespace _Ekko.Scripts.Gameplay.Utils
{
    public class GameplayZone : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private MovementModeType movementMode;
    
        [Header("Side Scroll Settings")]
        [SerializeField] private SideScrollDirection sideScrollDirection;
    
        [Header("References")]
        [SerializeField] private PlayerController3D player;
        [SerializeField] private CameraManager cameraManager;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
        
            player.SetMovementMode(movementMode);
            cameraManager.SetMode(movementMode);
        
            // Si platformer, set aussi la direction
            if (movementMode == MovementModeType.Platformer)
            {
                SideScrollManager.Instance.SetDirection(sideScrollDirection);
            }
        }

        private void OnDrawGizmos()
        {
            Color color = movementMode switch
            {
                MovementModeType.Platformer => Color.blue,
                MovementModeType.TopDown => Color.green,
                MovementModeType.Free => Color.red,
                _ => Color.white
            };
        
            color.a = 0.3f;
            Gizmos.color = color;
        
            var boxCollider = GetComponent<BoxCollider>();
            if (boxCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            }
        
            // Dessine direction si platformer
            if (movementMode == MovementModeType.Platformer)
            {
                Gizmos.color = Color.yellow;
                Vector3 dir = sideScrollDirection == SideScrollDirection.X ? Vector3.right : Vector3.forward;
                Gizmos.DrawRay(transform.position, dir * 3f);
            }
        }
    }
}
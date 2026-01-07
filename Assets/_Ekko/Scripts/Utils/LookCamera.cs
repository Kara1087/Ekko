using UnityEngine;

namespace _Ekko.Scripts.Utils
{
    public class LookCamera : MonoBehaviour
    {
        
        private enum Direction
        {
            Forward,
            Backward,
            Left,
            Right,
            Up,
            Down
        }
        
        [SerializeField] 
        private Direction localAxisTowardCamera;
        
        private Vector3 localAxis;
        private Quaternion axisCorrection;
        private Camera mainCam;

        private void Awake()
        {
            UpdateLocalAxis();
        }

        private void Start()
        {
            mainCam = Camera.main;
        }

        [ContextMenu("Update Local Axis")]
        public void UpdateLocalAxis()
        {
            localAxis = localAxisTowardCamera switch
            {
                Direction.Forward => Vector3.forward,
                Direction.Backward => Vector3.back,
                Direction.Left => Vector3.left,
                Direction.Right => Vector3.right,
                Direction.Up => Vector3.up,
                Direction.Down => Vector3.down,
                _ => Vector3.forward
            };
            axisCorrection = Quaternion.FromToRotation(localAxis, Vector3.forward);
        }

        private void LateUpdate()
        {
            Vector3 directionToCamera = mainCam.transform.position - transform.position;
            
            // Calcule la rotation pour que _localAxis pointe vers la caméra
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera) * axisCorrection;
            
            transform.rotation = targetRotation;
        }
    }
}
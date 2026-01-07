using Unity.Cinemachine;
using UnityEngine;

namespace _Ekko.Scripts.Gameplay.CharacterController
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera platformerCam;
        [SerializeField] private CinemachineCamera topDownCam;
        [SerializeField] private CinemachineCamera freeCam;

        public void SetMode(MovementModeType mode)
        {
            platformerCam.Priority = 0;
            topDownCam.Priority = 0;
            freeCam.Priority = 0;

            switch (mode)
            {
                case MovementModeType.Platformer:
                    platformerCam.Priority = 10;
                    break;
                case MovementModeType.TopDown:
                    topDownCam.Priority = 10;
                    break;
                case MovementModeType.Free:
                    freeCam.Priority = 10;
                    break;
            }
        }
    }
}
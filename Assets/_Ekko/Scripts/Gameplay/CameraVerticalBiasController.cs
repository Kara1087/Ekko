using UnityEngine;
using Unity.Cinemachine;

public class CameraVerticalBiasController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private CinemachinePositionComposer positionComposer;
    [SerializeField] private CinemachineCamera virtualCamera;

    [Header("Settings")]
    [SerializeField] private float screenYUp = 1f;              // Quand on saute
    [SerializeField] private float screenYDown = -1f;           // Quand on chute
    [SerializeField] private float transitionSpeed = 5f;        // Lissage
    [SerializeField] private float fallTransitionSpeed = 10f;   // transition plus rapide vers le bas
    private float targetScreenY = 0f;

    // État actuel de la caméra : montée, descente, neutre
    private enum CameraBiasState { Neutral, Jumping, Falling }
    private CameraBiasState currentState = CameraBiasState.Neutral;

    private void Start()
    {
        if (playerRb == null) Debug.LogError("❌ Player Rigidbody2D manquant !");
        if (positionComposer == null) Debug.LogError("❌ Cinemachine Position Composer manquant !");
        if (virtualCamera == null) Debug.LogError("❌ VirtualCamera non assignée !");
    }

    private void Update()
    {
        if (playerRb == null || positionComposer == null) return;

        float verticalVelocity = playerRb.linearVelocity.y;
        Vector2 currentScreenPos = positionComposer.Composition.ScreenPosition;

        // Log continu pour suivre la position Y à l'écran

        // État mis à jour uniquement quand on change vraiment d’état
        if (verticalVelocity > 1f && currentState != CameraBiasState.Jumping)
        {
            currentState = CameraBiasState.Jumping;
            targetScreenY = screenYUp;
        }
        else if (verticalVelocity < -1f && currentState != CameraBiasState.Falling)
        {
            currentState = CameraBiasState.Falling;
            targetScreenY = screenYDown;
        }
        else if (Mathf.Abs(verticalVelocity) < 0.1f && currentState != CameraBiasState.Neutral)
        {
            currentState = CameraBiasState.Neutral;
            targetScreenY = 0f; // Centre de l'écran
        }

        // On utilise une vitesse différente pour la chute
        float usedSpeed = (currentState == CameraBiasState.Falling) ? fallTransitionSpeed : transitionSpeed;

        currentScreenPos.y = Mathf.MoveTowards(
            currentScreenPos.y,
            targetScreenY,
            Time.deltaTime * usedSpeed
        );

        positionComposer.Composition.ScreenPosition = currentScreenPos;
        
    }

}
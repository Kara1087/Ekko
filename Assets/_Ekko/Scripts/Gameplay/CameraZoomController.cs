using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

/// <summary>
/// Gère le zoom et le recentrage de la caméra au démarrage.
/// Compatible Cinemachine 3.1 (modular stack).
/// </summary>
public class CameraZoomController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineCamera virtualCamera;

    [Header("Zoom settings")]
    [SerializeField] private float zoomedInSize = 2f;                       // Taille initiale du zoom (plan rapproché)
    [SerializeField] private float zoomedOutSize = 6f;                      // Taille cible du zoom-out
    [SerializeField] private float zoomDuration = 2f;                       // Durée de l’animation de zoom

    [Header("Offset settings")]

    [SerializeField] private Transform playerTransform;                     // 🔍 Référence au joueur (à assigner dans l'inspector)
    private CinemachinePositionComposer positionComposer;                   // Contrôle de l’offset cible de la caméra
    private Coroutine zoomCoroutine;                                        // Pour éviter de lancer plusieurs coroutines simultanément

    private void Awake()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera non assignée !");
            return;
        }

        // Accès au composant PositionComposer (modulaire)
        positionComposer = virtualCamera.GetComponentInChildren<CinemachinePositionComposer>();
        if (positionComposer == null)
        {
            Debug.LogError("PositionComposer non trouvé sur la CinemachineCamera !");
            return;
        }

        // Initialisation au zoom rapproché
        virtualCamera.Lens.OrthographicSize = zoomedInSize;
        Debug.Log($"[CameraZoomController] ✅ Caméra initialisée à zoomIn={zoomedInSize}");
    }

    private void Start()
    {
        if (playerTransform != null)
        {
            StartCoroutine(DelayedZoomOut(1.5f));
        }
        else
        {
            Debug.LogWarning("[CameraZoomController] ⚠️ PlayerTransform non assigné !");
        }
    }

    /// <summary>
    /// Coroutine qui attend un certain délai avant de lancer le zoom-out.
    /// </summary>
    private IEnumerator DelayedZoomOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerZoomOut();
    }

    /// <summary>
    /// Déclenche un zoom-out avec recadrage (offset recentré).
    /// </summary>
    public void TriggerZoomOut()
    {
        // On interrompt une éventuelle animation précédente
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        // On lance une nouvelle animation de zoom
        zoomCoroutine = StartCoroutine(ZoomAndRecenter(zoomedOutSize));
    }

    /// <summary>
    /// Coroutine qui anime le zoom (size) de la caméra
    /// </summary>
    private IEnumerator ZoomAndRecenter(float targetSize)
    {
        float startSize = virtualCamera.Lens.OrthographicSize;
        float timer = 0f;

        while (timer < zoomDuration)
        {
            timer += Time.deltaTime;
            float t = timer / zoomDuration;

            // Interpolation du zoom
            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }

        // Valeur finale
        virtualCamera.Lens.OrthographicSize = targetSize;
    }

    /// <summary>
    /// Déclenche un zoom vers une taille cible (sans changer l'offset).
    /// Appelable depuis un trigger ou un événement.
    /// </summary>
    public void TriggerZoom(float targetSize, float duration)
    {
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomOnly(targetSize, duration));
    }

    /// <summary>
    /// Coroutine qui anime uniquement la taille de la caméra sans changer l’offset.
    /// </summary>
    private IEnumerator ZoomOnly(float targetSize, float duration)
    {
        float startSize = virtualCamera.Lens.OrthographicSize;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }

        virtualCamera.Lens.OrthographicSize = targetSize;
    }
}
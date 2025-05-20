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
    [SerializeField] private float zoomedInSize = 2f;
    [SerializeField] private float zoomedOutSize = 6f;
    [SerializeField] private float zoomDuration = 2f;

    [Header("Offset settings")]
    [SerializeField] private Vector3 offsetZoomIn = new Vector3(0f, 1f, 0f);
    [SerializeField] private Vector3 offsetZoomOut = Vector3.zero;

    private CinemachinePositionComposer positionComposer;
    private Coroutine zoomCoroutine;

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
        positionComposer.TargetOffset = offsetZoomIn;
    }

    private void Start()
    {
        // Lancement automatique du zoom-out après 1.5 secondes
        StartCoroutine(DelayedZoomOut(1.5f));
    }

    private IEnumerator DelayedZoomOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerZoomOut();
    }

    public void TriggerZoomOut()
    {
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomAndRecenter(zoomedOutSize, offsetZoomOut));
    }

    private IEnumerator ZoomAndRecenter(float targetSize, Vector3 targetOffset)
    {
        float startSize = virtualCamera.Lens.OrthographicSize;
        Vector3 startOffset = positionComposer.TargetOffset;
        float timer = 0f;

        while (timer < zoomDuration)
        {
            timer += Time.deltaTime;
            float t = timer / zoomDuration;

            // Interpolation du zoom et de l'offset
            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
            positionComposer.TargetOffset = Vector3.Lerp(startOffset, targetOffset, t);

            yield return null;
        }

        // Valeurs finales
        virtualCamera.Lens.OrthographicSize = targetSize;
        positionComposer.TargetOffset = targetOffset;
    }
}
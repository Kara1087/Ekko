using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Caméras")]
    [SerializeField] private CinemachineCamera focusCam; // Caméra fixe sur la porte
    [SerializeField] private CinemachineCamera followCam; // Caméra principale du joueur

    [Header("Timing")]
    [SerializeField] private float focusDuration = 2f; // Temps pendant lequel on garde le focus

    /// <summary>
    /// Lance la transition vers la caméra focus, puis revient à la caméra joueur.
    /// </summary>
    public void SwitchToFocus()
    {
        if (focusCam == null || followCam == null) return;

        focusCam.Priority = 20;
        followCam.Priority = 10;

        StartCoroutine(ReturnToFollow());
    }

    private IEnumerator ReturnToFollow()
    {
        yield return new WaitForSeconds(focusDuration);

        focusCam.Priority = 10;
        followCam.Priority = 20;
    }
}
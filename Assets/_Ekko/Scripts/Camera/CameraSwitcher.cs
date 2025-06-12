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
    /// Transition smooth (blend habituel) vers la caméra focus, puis revient à la caméra joueur.
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


    /// <summary>
    /// Transition instantanée (cut) vers la caméra focus, puis revient à la caméra joueur.
    /// </summary>
    public void SwitchToFocusInstant()
    {
        // Étape 1 : Récupère le CinemachineBrain attaché à la caméra principale
        var brain = Camera.main.GetComponent<CinemachineBrain>();

        // Étape 2 : Stocke le blend original pour pouvoir le restaurer plus tard
        var originalBlend = brain.DefaultBlend;

        // Étape 3 : Applique un blend instantané (Cut)
        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);

        // Étape 4 : Active la caméra de focus
        focusCam.Priority = 20;
        followCam.Priority = 10;

        // Étape 5 : Lance la coroutine pour revenir à la caméra de suivi après un délai
        StartCoroutine(ReturnToFollow());

        // Étape 6 : Restaure le blend original après un court délai (temps nécessaire au switch)
        StartCoroutine(RestoreBlendAfterDelay(brain, originalBlend, 0.1f));
    }

    private IEnumerator RestoreBlendAfterDelay(CinemachineBrain brain, CinemachineBlendDefinition originalBlend, float delay)
    {
        yield return new WaitForSeconds(delay);
        brain.DefaultBlend = originalBlend;
    }
}
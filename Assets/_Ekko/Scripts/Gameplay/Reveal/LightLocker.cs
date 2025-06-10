using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Composant qui contrôle une Light2D en tant que feedback visuel d’un événement de type "dommage".
/// Implémente l’interface `IDamagerFeedback`, ce qui permet au `Damager` de l’activer ou le désactiver.
/// Très utile pour créer des **effets de lumière contextuels** (ex: laser qui s’allume quand on entre dans une zone).
/// </summary>

public class LightLocker : MonoBehaviour, IDamagerFeedback
{
    [Header("Target Light")]
    [SerializeField] private Light2D targetLight;         // Lumière 2D à contrôler (doit être assignée dans l’inspecteur)

    [Header("Valeurs ON")]
    [SerializeField] private float litIntensity = 1f;     // Intensité quand la lumière est "allumée"
    [SerializeField] private float litRadius = 5f;        // Rayon extérieur quand la lumière est "allumée"

    [Header("Valeurs OFF")]
    [SerializeField] private float offIntensity = 0f;     // Intensité quand la lumière est éteinte
    [SerializeField] private float offRadius = 0f;        // Rayon quand la lumière est éteinte

    private void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponentInChildren<Light2D>();
            Debug.LogWarning("[LightLocker] ⚠️ Light2D non assignée manuellement, tentative de récupération via GetComponentInChildren.");
        }
    }

    /// <summary>
    /// Méthode appelée par le `Damager` quand le joueur entre dans la zone
    /// </summary>

    public void TriggerFeedback()
    {
        if (targetLight == null)
        {
            Debug.LogWarning("[LightLocker] ❌ targetLight est null, impossible d’allumer la lumière");
            return;
        }

        targetLight.enabled = true;
        targetLight.intensity = litIntensity;
        targetLight.pointLightOuterRadius = litRadius;

        //Debug.Log($"[LightLocker] 💡 Activation lumière → Intensity: {litIntensity} | Radius: {litRadius}");

    }

    /// <summary>
    /// Méthode appelée par le `Damager` quand le joueur sort de la zone
    /// </summary>
    public void StopFeedback()
    {
        if (targetLight == null) return;

        targetLight.intensity = offIntensity;
        targetLight.pointLightOuterRadius = offRadius;
    }
}
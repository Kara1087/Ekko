using UnityEngine;

[RequireComponent(typeof(Transform))]
public class WaveEmitter : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private GameObject wavePrefab;
    [SerializeField] private float minRange = 1f;
    [SerializeField] private float maxRange = 16f;              // Plus grand rayon possible
    [SerializeField] private float rangePowerCurve = 1.5f;      // Exposant pour courbe puissance (1f = linéaire, 1.5f = exponentielle, 2f = quadratique)
    [SerializeField] private float rangeMultiplier = 1f;      // Modulation globale
    [SerializeField] private float minForce = 1f;
    [SerializeField] private float maxForce = 20f;
    
    [Header("Debug")]
    [SerializeField] private Color debugColor = Color.cyan;
    private float debugGizmoRadius = 1f;                        // Valeur par défaut
    
    private LandingClassifier landingClassifier;                // Pour accéder au dernier type d’atterrissage
    
    private void Awake()
    {
        landingClassifier = GetComponent<LandingClassifier>();
    }

    /// <summary>
    /// Appelé par JumpSystem à l’atterrissage. Calcule et émet une onde.
    /// </summary>

    public void EmitWave(float impactForce)
    {
        // 1. Calcul du rayon cible en fonction de la force d'impact
        float clampedForce = Mathf.Clamp(impactForce, minForce, maxForce);
        float t = Mathf.InverseLerp(minForce, maxForce, clampedForce);
        t = Mathf.Pow(t, rangePowerCurve);                              // Ajout de la courbe d’intensité
        float targetRadius = Mathf.Lerp(minRange, maxRange, t)* rangeMultiplier;

        // 2. Debug info
        // debugGizmoRadius = targetRadius;
        // LandingType type = landingClassifier.GetCurrentLandingType();
        // Debug.Log($"🌀 [WaveEmitter] Atterrissage {type} | Emission d'onde - Force: {impactForce:F2} → Rayon: {targetRadius:F2}");

        // 3. Instanciation de l'onde
        if (wavePrefab != null)
        {
            GameObject waveGO = Instantiate(wavePrefab, transform.position, Quaternion.identity);
            
            Wave wave = waveGO.GetComponent<Wave>();
            if (wave != null)
            {
                wave.Initialize(impactForce, targetRadius, minForce, maxForce); // Important : transmettre l’impact pour la fadeSpeed dynamique
            }
        }


        // 4. TODO : Réactions (revealables, IA, triggers…)
    }

    /// <summary>
    /// Affiche un Gizmo représentant la portée de la dernière onde.
    /// </summary>
    /*private void OnDrawGizmos()
    {
        Gizmos.color = debugColor;
        Gizmos.DrawWireSphere(transform.position, debugGizmoRadius);
    }*/

}

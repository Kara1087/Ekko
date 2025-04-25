using UnityEngine;

[RequireComponent(typeof(Transform))]
public class WaveEmitter : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private GameObject wavePrefab;
    // bornes entre lesquelles rayon est interpolé
    [SerializeField] private float minRange = 1f;
    [SerializeField] private float maxRange = 16f;          // Plus grand rayon possible
    [SerializeField] private float rangePowerCurve = 1.5f; // Exposant pour courbe puissance (1f = linéaire, 1.5f = exponentielle, 2f = quadratique)
    // échelle onde via impactVelocity
    [SerializeField] private float minForce = 1f;
    [SerializeField] private float maxForce = 20f;
    [SerializeField] private float lifetime = 2f;           // Durée de vie du prefab instancié
    
    [Header("Debug")]
    [SerializeField] private Color debugColor = Color.cyan;
    private float debugGizmoRadius = 1f; // Valeur par défaut
    private LandingClassifier landingClassifier;          // Pour accéder au dernier type d’atterrissage


    
    private void Awake()
    {
        landingClassifier = GetComponent<LandingClassifier>();
    }

    /// <summary>
    /// Appelé par JumpSystem à l’atterrissage. Calcule et émet une onde.
    /// </summary>

    public void EmitWave(float impactForce)
    {
        // 1. Clamp et mapping de la force
        float clampedForce = Mathf.Clamp(impactForce, minForce, maxForce);
        float t = Mathf.InverseLerp(minForce, maxForce, clampedForce);
        t = Mathf.Pow(t, rangePowerCurve);                              // Ajout de la courbe d’intensité
        float radius = Mathf.Lerp(minRange, maxRange, t);

        // 2. Debug info
        debugGizmoRadius = radius;
        LandingType type = landingClassifier.GetCurrentLandingType();
        Debug.Log($"🌀 [WaveEmitter] Atterrissage {type} | Emission d'onde - Force: {impactForce:F2} → Rayon: {radius:F2}");

        // 3. FX (en attente prefab)
        if (wavePrefab != null)
        {
            GameObject wave = Instantiate(wavePrefab, transform.position, Quaternion.identity);
            wave.transform.localScale = Vector3.one * radius;

            Destroy(wave, lifetime); // Auto-destruction après délai
        }


        // 4. TODO : Réactions (revealables, IA, triggers…)
    }

    /// <summary>
    /// Affiche un Gizmo représentant la portée de la dernière onde.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = debugColor;
        Gizmos.DrawWireSphere(transform.position, debugGizmoRadius);
    }

}

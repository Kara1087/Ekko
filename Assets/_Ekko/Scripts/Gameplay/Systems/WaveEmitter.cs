using UnityEngine;


public class WaveEmitter : MonoBehaviour, ILandingListener
{
    [Header("Wave Settings")]
    [SerializeField] private GameObject wavePrefab;         // Prefab d’onde à instancier lors d’un atterrissage
    [SerializeField] private float minRange = 1f;           // Rayon minimum de l’onde (pour les petits impacts)
    [SerializeField] private float maxRange = 16f;          // Rayon maximum de l’onde (pour les gros impacts)
    [SerializeField] private float rangePowerCurve = 1.5f;  // Contrôle la courbe d’expansion (1 = linéaire, >1 = exponentiel)
    [SerializeField] private float rangeMultiplier = 1f;    // Permet de scaler dynamiquement toutes les ondes (ex: bonus temporaire)

    [SerializeField] private float minForce = 1f;           // Force minimale attendue à l’atterrissage
    [SerializeField] private float maxForce = 20f;          // Force maximale attendue à l’atterrissage


    [Header("Debug")]
    [SerializeField] private Color debugColor = Color.cyan;
    private float debugGizmoRadius = 1f;                        // Valeur par défaut

    private JumpSystem jumpSystem;

    private void OnEnable()
    {
        if (jumpSystem == null)
            jumpSystem = FindFirstObjectByType<JumpSystem>();

        if (jumpSystem != null)
            jumpSystem.RegisterLandingListener(this);
    }

    private void OnDisable()
    {
        if (jumpSystem != null)
            jumpSystem.UnregisterLandingListener(this);
    }

    public void OnLandingDetected(float impactForce, LandingType type)
    {
        Debug.Log($"[WaveEmitter] 🔊 Reçu impact {impactForce} depuis JumpSystem");
        EmitWave(impactForce);
    }

    /// <summary>
    /// Appelé lors de l’atterrissage par JumpSystem.
    /// Génère une onde avec un rayon proportionnel à la force de l’impact.
    /// </summary>
    public void EmitWave(float impactForce)
    {
        // 1. 🔒 Clamp la force pour qu’elle reste dans l’intervalle [minForce, maxForce]
        float clampedForce = Mathf.Clamp(impactForce, minForce, maxForce);

        // 2. 📈 Interpolation : convertit la force en facteur [0-1]
        float t = Mathf.InverseLerp(minForce, maxForce, clampedForce);

        // 3. 📊 Applique une courbe exponentielle pour rendre les petites forces plus douces et les grandes plus puissantes
        t = Mathf.Pow(t, rangePowerCurve);

        // 4. 📐 Calcule le rayon final de l’onde
        float targetRadius = Mathf.Lerp(minRange, maxRange, t) * rangeMultiplier;

        // 5. 🧪 Log debug (commenté ici, tu peux le décommenter si tu veux observer les valeurs dans la console)
        /*
        debugGizmoRadius = targetRadius; // Pour visualiser l’onde dans OnDrawGizmos
        LandingType type = landingClassifier.GetCurrentLandingType();
        Debug.Log($"🌀 [WaveEmitter] Atterrissage {type} | Emission d'onde - Force: {impactForce:F2} → Rayon: {targetRadius:F2}");
        */

        // 6. 🌀 Instancie l’onde à la position du joueur
        if (wavePrefab != null)
        {
            GameObject waveGO = Instantiate(wavePrefab, transform.position, Quaternion.identity);

            // 7. Initialise les paramètres de l’onde (rayon + impact → utile pour effets visuels dynamiques)
            Wave wave = waveGO.GetComponent<Wave>();
            if (wave != null)
            {
                wave.Initialize(impactForce, targetRadius, minForce, maxForce);
            }
        }

        // 8. TODO : ici tu pourras déclencher des réactions
        // - Révéler les éléments (Revealables)
        // - Alerter les ennemis
        // - Activer des pièges ou effets sonores
    }

    /// <summary>
    /// 🔍 Affiche dans la scène (éditeur uniquement) un cercle représentant la dernière onde
    /// </summary>
    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = debugColor;
        Gizmos.DrawWireSphere(transform.position, debugGizmoRadius);
    }
    */

}

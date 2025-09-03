using UnityEngine;


public class WaveEmitter : MonoBehaviour, ILandingListener
{
    [Header("Wave Settings")]
    [SerializeField] private float minRange = 1f;           // Rayon minimum de l’onde (pour les petits impacts)
    [SerializeField] private float maxRange = 16f;          // Rayon maximum de l’onde (pour les gros impacts)
    [SerializeField] private float rangePowerCurve = 1.5f;  // Contrôle la courbe d’expansion (1 = linéaire, >1 = exponentiel)
    [SerializeField] private float rangeMultiplier = 1f;    // Permet de scaler dynamiquement toutes les ondes (ex: bonus temporaire)

    [SerializeField] private float minForce = 1f;           // Force minimale attendue à l’atterrissage
    [SerializeField] private float maxForce = 20f;          // Force maximale attendue à l’atterrissage


    [Header("Debug")]
    [SerializeField] private Color debugColor = Color.cyan;

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

    public void OnLandingDetected(float impactForce, LandingType type, Transform landObject)
    {
        //sDebug.Log($"[WaveEmitter] 🔊 Reçu impact {impactForce} depuis JumpSystem");
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

        // 6. 🌀 Instancie l’onde à la position du joueur
        FXManager.Instance.PlayWaveFX(transform.position, impactForce,minForce, maxForce, targetRadius, Quaternion.identity);

    }

}

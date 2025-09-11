using _Ekko.Scripts.Gameplay.Systems;
using UnityEngine;


public class WaveEmitter : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float minRange = 1f;           // Rayon minimum de l’onde (pour les petits impacts)
    [SerializeField] private float maxRange = 16f;          // Rayon maximum de l’onde (pour les gros impacts)
    [SerializeField] private float rangePowerCurve = 1.5f;  // Contrôle la courbe d’expansion (1 = linéaire, >1 = exponentiel)
    [SerializeField] private float rangeMultiplier = 1f;    // Permet de scaler dynamiquement toutes les ondes (ex: bonus temporaire)

    [SerializeField] private float minForce = 1f;           // Force minimale attendue à l’atterrissage
    [SerializeField] private float maxForce = 20f;          // Force maximale attendue à l’atterrissage
    

    private void OnEnable()
    {
        EventManager.Subscribe(GameEventType.PlayerLand, OnPlayerLand);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe(GameEventType.PlayerLand, OnPlayerLand);
    }

    private void OnPlayerLand(object obj)
    {
        LandData data = (LandData)obj;
        EmitWave(data.force);
    }

    /// <summary>
    /// Appelé lors de l’atterrissage par JumpSystem.
    /// Génère une onde avec un rayon proportionnel à la force de l’impact.
    /// </summary>
    private void EmitWave(float impactForce)
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

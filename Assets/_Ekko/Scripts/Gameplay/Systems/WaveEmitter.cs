using _Ekko.Scripts.Gameplay.Systems;
using UnityEngine;


public class WaveEmitter : MonoBehaviour
{
    [Header("Wave Prefab")]
    [SerializeField] private Wave wavePrefab;

    [Header("Wave Settings")]
    [SerializeField] private float minRange = 1f;           // Rayon minimum de l’onde (pour les petits impacts)
    [SerializeField] private float maxRange = 16f;          // Rayon maximum de l’onde (pour les gros impacts)
    [SerializeField] private float rangePowerCurve = 1.5f;  // Contrôle la courbe d’expansion (1 = linéaire, >1 = exponentiel)
    [SerializeField] private float rangeMultiplier = 1f;    // Permet de scaler dynamiquement toutes les ondes (ex: bonus temporaire)

    [SerializeField] private float minForce = 1f;           // Force minimale attendue à l’atterrissage
    [SerializeField] private float maxForce = 20f;          // Force maximale attendue à l’atterrissage
    
    
    [Space(5),Header("Cushion Settings")]
    [SerializeField] private float cushionMusicFadeTime = 0.5f;
    [SerializeField] private float cushionTargetVolume = 0.1f;

    private float originalMusicVolume = 1f;
    private bool isFading = false;


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
        EmitWave(data.force, data.landingType);
    }

    /// <summary>
    /// Appelé lors de l’atterrissage par JumpSystem.
    /// Génère une onde avec un rayon proportionnel à la force de l’impact.
    /// </summary>
    private void EmitWave(float impactForce, LandingType landingType)
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
        // Spawn wave directly
        Wave wave = ObjectPoolManager.SpawnObject(wavePrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Wave);
        wave.Initialize(impactForce, targetRadius, minForce, maxForce);
        
        // Play distortion effect directly
        //waveDistortionController?.PlayDistortion(impactForce, minForce, maxForce);
        //TODO Continue this implementation to apply rumble camera shake etc...
        // Play audio directly
        //AudioManager.Instance?.Play(waveAudioName, transform.position);
        PlaySound(impactForce, landingType);
    }

    private void PlaySound(float impactForce, LandingType landingType)
    {
        if (LandingUtils.IsHeavyImpact(impactForce, landingType))
        {
            AudioManager.Instance.Play("SlamJump");
            //CameraShakeManager.Instance?.Shake(); // si déjà implémenté
        }

        if (landingType == LandingType.Cushioned && !isFading)
        {
            isFading = true;
            originalMusicVolume = AudioManager.Instance.GetCurrentMusicVolume();
            AudioManager.Instance.SetVolume("BackgroundTheme", cushionTargetVolume);
            Invoke(nameof(RestoreVolume), cushionMusicFadeTime);
        }
    }
    
    private void RestoreVolume()
    {
        AudioManager.Instance.SetVolume("BackgroundTheme", originalMusicVolume);
        isFading = false;
    }
    
    /*public void TriggerCameraShake()
   {
       if (impulseSource != null)
       impulseSource.GenerateImpulse();
       else
           Debug.LogWarning("[LandingReactionManager] 🎥 impulseSource non assigné !");
   }

   public void TriggerMotionBlur()
   {
       if (globalVolume.profile.TryGet<MotionBlur>(out var motionBlur))
       {
           motionBlur.active = true;
           motionBlur.intensity.Override(1f); // valeur forte (0.7 à 1)
           Invoke(nameof(DisableMotionBlur), motionBlurDuration);
       }
   }

   private void DisableMotionBlur()
   {
       if (globalVolume.profile.TryGet<MotionBlur>(out var motionBlur))
           motionBlur.active = false;
   }*/
}

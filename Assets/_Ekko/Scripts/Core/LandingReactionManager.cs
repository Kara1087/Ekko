using _Ekko.Scripts.Gameplay.Systems;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LandingReactionManager : MonoBehaviour
{
    [Header("Cushion Settings")]
    [SerializeField] private float cushionMusicFadeTime = 0.5f;
    [SerializeField] private float cushionTargetVolume = 0.1f;
    
    //[SerializeField] private CinemachineImpulseSource impulseSource;

    //[SerializeField] private Volume globalVolume;
    //[SerializeField] private float motionBlurDuration = 0.25f;
    [Header("Events")]
    public UnityEvent onHeavyLanding;

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
        
        //TODO think and check with Karine the landing type is really necessary ?
        OnLandingDetected(data.force, data.landingType);
    }

    private void OnLandingDetected(float force, LandingType type)
    {
        //TODO think about it where is better to apply all this effects
        //Debug.Log($"[LandingAudioCue] 📥 Reçu : type={type}, force={force}");

        if (LandingUtils.IsHeavyImpact(force, type))
        {
            onHeavyLanding?.Invoke();
            AudioManager.Instance.Play("SlamJump");
            //CameraShakeManager.Instance?.Shake(); // si déjà implémenté
        }

        if (type == LandingType.Cushioned && !isFading)
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

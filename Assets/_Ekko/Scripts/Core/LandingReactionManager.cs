using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LandingReactionManager : MonoBehaviour, ILandingListener
{
    [Header("Cushion Settings")]
    [SerializeField] private float cushionMusicFadeTime = 0.5f;
    [SerializeField] private float cushionTargetVolume = 0.1f;
    [Header("Slam Settings")]
    [SerializeField] private float slamThreshold = 25f;
    //[SerializeField] private CinemachineImpulseSource impulseSource;

    //[SerializeField] private Volume globalVolume;
    //[SerializeField] private float motionBlurDuration = 0.25f;
    [Header("Events")]
    public UnityEvent onHeavyLanding;

    private float originalMusicVolume = 1f;
    private bool isFading = false;

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


    public void OnLandingDetected(float force, LandingType type, Transform landObject)
    {
        //Debug.Log($"[LandingAudioCue] 📥 Reçu : type={type}, force={force}");

        if (LandingUtils.IsHeavyImpact(force, type, slamThreshold))
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

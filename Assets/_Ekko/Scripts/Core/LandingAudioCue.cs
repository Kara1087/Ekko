using UnityEngine;

public class LandingAudioCue : MonoBehaviour, ILandingListener
{
    [SerializeField] private float cushionMusicFadeTime = 0.5f;
    [SerializeField] private float cushionTargetVolume = 0.1f;
    [SerializeField] private float slamSoundThreshold = 25f;

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

        switch (type)
        {
            case LandingType.Slam:
                AudioManager.Instance.Play("SlamJump");
                break;

            case LandingType.Cushioned:
                if (!isFading)
                {
                    isFading = true;
                    originalMusicVolume = AudioManager.Instance.GetCurrentMusicVolume();
                    AudioManager.Instance.SetVolume("BackgroundTheme", cushionTargetVolume);
                    Invoke(nameof(RestoreVolume), cushionMusicFadeTime);
                }
                break;
                
            default:
            if (force >= slamSoundThreshold)
            {
                AudioManager.Instance.Play("SlamJump");
            }
            break;
        }
    }

    private void RestoreVolume()
    {
        AudioManager.Instance.SetVolume("BackgroundTheme", originalMusicVolume);
        isFading = false;
    }
    
}

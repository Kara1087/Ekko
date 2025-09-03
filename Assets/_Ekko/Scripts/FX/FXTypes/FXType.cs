using UnityEngine;

public enum FXCategory { Sound, VFX, Splash, Wave, Haptic }

[CreateAssetMenu(menuName = "FX/FX Type")]
public class FXType : ScriptableObject
{
    public FXCategory category;
    public AudioClip audioClip;
    public float audioVolume = 1f;
    public float autoReturnTime = 2f; // For pooling cleanup
}
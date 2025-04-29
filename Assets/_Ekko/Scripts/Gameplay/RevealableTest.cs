using UnityEngine;

public class RevealableTest : MonoBehaviour, IRevealable
{
    public void Reveal(float waveIntensity)
    {
        Debug.Log($"[RevealableTest] Je suis touché par une onde avec intensité {waveIntensity} !");
    }
}
// Assets/Scripts/Core/LandingClassifier.cs

using UnityEngine;

public class LandingClassifier : MonoBehaviour
{
    [Header("Debug")]
    public LandingType currentLandingType;
    public float lastImpactVelocity;

    public void RegisterLanding(float impactVelocity, LandingType type)
    {
        lastImpactVelocity = Mathf.Abs(impactVelocity);
        currentLandingType = type;

         Debug.Log($"[LandingClassifier] Nouveau type d’atterrissage : {type} | Impact : {lastImpactVelocity:F2}");
    }

    public LandingType GetCurrentLandingType() => currentLandingType;

    public float GetLastImpactVelocity() => lastImpactVelocity;
}
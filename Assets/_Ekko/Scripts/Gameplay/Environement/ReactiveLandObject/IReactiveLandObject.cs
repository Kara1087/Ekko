using UnityEngine;

public interface IReactiveLandObject
{
    void Initialize();
    void OnLandingDetected(float landForce, LandingType landingType, Transform landObject, Transform playerTransform);
}
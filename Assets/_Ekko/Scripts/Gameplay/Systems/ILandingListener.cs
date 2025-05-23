// ILandingListener.cs

using UnityEngine;


/// <summary>
/// Interface pour les objets qui souhaitent recevoir des notifications d'atterrissage.
/// </summary>
public interface ILandingListener
{
    void OnLandingDetected(float impactForce, LandingType type, Transform landObject);
}
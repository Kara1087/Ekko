using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Synchronise dynamiquement le rayon d’un système de particules
/// avec la lumière d’un Light2D (type Point).
/// Cela permet de créer un effet cohérent visuellement entre les particules
/// (ex : un halo de lumière ou une aura) et la lumière réelle.
/// </summary>

public class ParticleSync : MonoBehaviour
{
    [SerializeField] private Light2D targetLight;       // Lumière 2D (Unity URP) à suivre
    [SerializeField] private ParticleSystem targetParticleSystem; // Système de particules à synchroniser
    [SerializeField] private float radiusMultiplier = 0.9f; // Facteur d’ajustement (ex : un peu plus petit que la lumière)

    // Accès direct au module "shape" du système de particules
    private ParticleSystem.ShapeModule shape;

    void Awake()
    {
        // Si aucune référence donnée manuellement, on cherche automatiquement sur le GameObject
        if (targetParticleSystem == null)
            targetParticleSystem = GetComponent<ParticleSystem>();

        // On accède au module de forme pour pouvoir modifier le rayon dynamiquement
        shape = targetParticleSystem.shape;
    }

    void Update()
    {
        // Si une lumière est bien assignée
        if (targetLight != null)
        {
            // Met à jour le rayon du système de particules en fonction du rayon de la lumière
            shape.radius = targetLight.pointLightOuterRadius * radiusMultiplier;
        }
    }
}
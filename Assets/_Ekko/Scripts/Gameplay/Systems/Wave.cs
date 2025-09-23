using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CircleCollider2D))]
public class Wave : MonoBehaviour
{
    #region Constants
    private const float LIGHT_ACTIVATION_THRESHOLD = 10f;
    private const float STARTING_RADIUS_RATIO = 0.3f;
    private const float DESTRUCTION_DELAY = 0.2f;
    private const float SCAN_INTERVAL = 0.2f; // Scan every 200ms instead of every frame
    #endregion
    
    [Header("Layer Masks")]
    [SerializeField] private LayerMask revealableLayers;          // Couches contenant les objets à révéler
    [SerializeField] private LayerMask alertableLayers;           // Couches contenant les ennemis ou objets à alerter
    
    [Header("Wave Settings"),]
    [SerializeField] private float waveMaxExpansionDuration = 2.5f;
    [SerializeField, Range(0.01f, 1f),Tooltip("Percentage of wave Duration to get max collider max Range")]
    private float colliderMaxRangeDuration = 2.5f;
    [SerializeField] private Ease colliderExpansionCurve;
    
    [Space(10),Header("Light Settings")]
    [SerializeField] private float lightIntensityFactor = 0.2f;   // Intensité maximale de la lumière
    [SerializeField,Range(0f,1f)] private float intensityMinRatio = 0.2f;      // Ratio pour calculer l’intensité minimale en fade
    [SerializeField, Range(0.01f, 1f),Tooltip("Percentage of wave Duration to get max intensity")]
    private float lightMaxIntensityDuration;
    [SerializeField] private Ease lightIntensityCurve;
    [Space(10)]
    
    [SerializeField, Range(0.01f, 1f), Tooltip("Percentage of wave Duration to get max radius")] 
    private float changeRadiusFactor = 0.2f;
    [SerializeField] private Ease lightRadiusCurve;
    [Space(10)]
    [SerializeField] private float disappearanceLightDuration;
    [SerializeField] private AnimationCurve disappearanceCurve;
    
    
    [Header("\nParticle Settings")]
    [SerializeField] private float particleMatchSizeFactor = 4f;
    [SerializeField, Space(10)] private float particleStartPlaybackSpeed = 10f;
    [SerializeField] private float particleEndPlaybackSpeed = 3f;
    [SerializeField] private float playbackChangeDuration;
    [SerializeField] private Ease playbackDurationCurve;
    
    [SerializeField,Space(10)] private float particleStartEmissionQuantity = 3f;
    [SerializeField] private float emissionDuration;
    [SerializeField] private Ease emissionDurationCurve;
    
    
    // Wave properties
    private float targetRadius;
    private float waveIntensity = 1f;
    
    // Component references (cached)
    private CircleCollider2D waveCollider;
    private ParticleSystem waveParticle;
    private Light2D waveLight;
    
    // Cached particle system modules
    private ParticleSystem.MainModule particleMain;
    private ParticleSystem.EmissionModule particleEmission;
    
    // Collections for affected objects (to avoid duplicate processing)
    private HashSet<GameObject> processedRevealables = new HashSet<GameObject>();
    private HashSet<GameObject> processedAlertables = new HashSet<GameObject>();
    
    // Animation Sequence
    private Sequence waveSequence;

    #region init
    private void Awake()
    {
        CacheComponents();
        InitializeCollider();
    }
    
    private void CacheComponents()
    {
        waveCollider = GetComponent<CircleCollider2D>();
        waveParticle = GetComponentInChildren<ParticleSystem>();
        waveLight = GetComponentInChildren<Light2D>();

        if (waveParticle != null)
        {
            particleMain = waveParticle.main;
            particleEmission = waveParticle.emission;
        }
    }
    private void InitializeCollider()
    {
        if (waveCollider != null)
        {
            waveCollider.isTrigger = true;
        }
    }
    #endregion
    
    #region Initialization
    /// <summary>
    /// Initialise l’onde avec des paramètres dynamiques selon la force.
    /// </summary>
    public void Initialize(float impactForce, float assignedTargetRadius, float minForce = 1f, float maxForce = 20f)
    {
        ResetWaveState();
        
        
        targetRadius = assignedTargetRadius;
        
        float normalizedForce = Mathf.Clamp(impactForce, minForce, maxForce);
        waveIntensity = normalizedForce;
        
        float duration = CalculateWaveDuration(normalizedForce, minForce, maxForce);
        if (duration <= 0f)
        {
            Debug.Log("returning to pool!");
            ResetWaveForPool();
            ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.Wave);
            return;
        }
        
        ConfigureCollider(assignedTargetRadius);
        ConfigureParticles();
        ConfigureLight(impactForce);
        
        
        // Start wave animation
        StartWaveAnimation(duration);
    }

    private float CalculateWaveDuration(float normalizedForce, float minForce, float maxForce)
    {
        float t = (normalizedForce - minForce) / (maxForce - minForce);
        float clampedValue  = Mathf.Clamp01(t); // keep result between 0 and 1
        return clampedValue * waveMaxExpansionDuration;
    }

    private void ResetWaveState()
    {
        // Safely kill the sequence before resetting
        if (waveSequence != null && waveSequence.IsActive())
        {
            waveSequence.Kill();
        }
        waveSequence = null;
        
        processedRevealables.Clear();
        processedAlertables.Clear();

    }
    

    private void ConfigureCollider(float assignedTargetRadius)
    {
        if (!waveCollider) return;
        
        waveCollider.radius = 0;
        
        if (waveParticle)
        {
            particleMain.startSize = assignedTargetRadius * STARTING_RADIUS_RATIO * particleMatchSizeFactor;;
        }
    }

    private void ConfigureParticles()
    {
        if (!waveParticle) return;
        
        particleEmission.rateOverTimeMultiplier = particleStartEmissionQuantity;
        particleMain.simulationSpeed = particleStartPlaybackSpeed;
    }

    private void ConfigureLight(float impactForce)
    {
        if (!waveLight) return;
        
        bool shouldEnableLight = impactForce >= LIGHT_ACTIVATION_THRESHOLD;
        waveLight.enabled = shouldEnableLight;

        if (shouldEnableLight && waveCollider)
        {
            waveLight.shapeLightFalloffSize = waveCollider.radius;
            float minIntensity = lightIntensityFactor * intensityMinRatio;
            waveLight.intensity = minIntensity;
        }
    }
    #endregion

    #region Wave Animation (Coroutines + Tweening)
    private void StartWaveAnimation(float percentageDuration)
    {
        waveSequence = DOTween.Sequence();

        if (waveCollider)
        {
            waveCollider.radius = 0.05f;
            var expansionTween = DOTween.To(
                () => waveCollider.radius,
                radius => waveCollider.radius = radius,
                targetRadius * 0.5f,
                (waveMaxExpansionDuration * percentageDuration) * colliderMaxRangeDuration
            ).SetEase(colliderExpansionCurve);
            
            waveSequence.Join(expansionTween);
        }
        
        // light animation if enabled
        if (waveLight && waveLight.enabled)
        {
            AnimateLightExpansion(percentageDuration);
        }

        if (waveParticle)
        {
            AnimateParticles(percentageDuration);
        }
        
        
        // Fade out light at the end
        waveSequence.Append(
            DOTween.To(
                () => waveLight.intensity,
                intensity => waveLight.intensity = intensity,
                0f, 
                disappearanceLightDuration * percentageDuration
            ).SetEase(disappearanceCurve)
        );
        
        waveSequence.OnComplete(ReturnWaveToPool);
        
        
        StartCoroutine(ObjectScanningCoroutine());
    }

    private void AnimateLightExpansion(float percentageDuration)
    {
        float maxIntensity = lightIntensityFactor;
        
        // Animate light intensity
        var intensityTween = DOTween.To(
            () => waveLight.intensity,
            intensity => waveLight.intensity = intensity,
            maxIntensity,
            (waveMaxExpansionDuration * lightMaxIntensityDuration) * percentageDuration
        ).SetEase(lightIntensityCurve);
        
        // Animate light radius to match wave expansion
        var radiusTween = DOTween.To(
            () => waveLight.shapeLightFalloffSize,
            radius => waveLight.shapeLightFalloffSize = radius,
            targetRadius * 0.5f,
            (waveMaxExpansionDuration* changeRadiusFactor) * percentageDuration
        ).SetEase(lightRadiusCurve);
        
        waveSequence.Join(intensityTween);
        waveSequence.Join(radiusTween);
        
    }
    
    
    private void AnimateParticles(float percentageDuration)
    {
        // Keep this manual - needs real-time tracking
        DOTween.To(() => particleMain.simulationSpeed, 
                x => particleMain.simulationSpeed = x, 
                particleEndPlaybackSpeed, 
                playbackChangeDuration * percentageDuration)
            .SetEase(playbackDurationCurve);
        
        DOTween.To(() => particleEmission.rateOverTimeMultiplier, 
                x => particleEmission.rateOverTimeMultiplier = x, 
                0f, 
                emissionDuration * percentageDuration)
            .SetEase(emissionDurationCurve)
            .From(particleStartEmissionQuantity);
    }
    
    private IEnumerator ObjectScanningCoroutine()
    {
        while (waveCollider.radius > 0f)
        {
            yield return new WaitForSeconds(SCAN_INTERVAL);
            
            if (waveCollider)
            {
                ScanForRevealables();
                ScanForAlertables();
            }
        }
    }

    #endregion

    private void ReturnWaveToPool()
    {
        StartCoroutine(DestroyAfterDelay(DESTRUCTION_DELAY));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Reset components before returning to pool
        ResetWaveForPool();
        ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.Wave);
    }

    private void ResetWaveForPool()
    {
        if (waveLight) waveLight.enabled = false;
        if (waveCollider) waveCollider.radius = 0f;
        if (waveParticle) waveParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        ResetWaveState();
    }

    #region Object Interaction

    //TODO Review this
    private void HandleRevealable(Collider2D other)
    {
        if (!IsInLayerMask(other.gameObject.layer, revealableLayers) || processedRevealables.Contains(other.gameObject)) return;

        if (other.TryGetComponent<IRevealable>(out var revealable) )
        {
            revealable.Reveal(waveIntensity);
            processedRevealables.Add(other.gameObject);;
        }
    }

    private void HandleAlertable(Collider2D other)
    {
        if (!IsInLayerMask(other.gameObject.layer, alertableLayers) || processedAlertables.Contains(other.gameObject)) return;
        
     
        if (other.TryGetComponent<IAlertable>(out var alertable))
        {
            alertable.Alert(transform.position);
            processedAlertables.Add(other.gameObject);
        }
    }

    private void ScanForRevealables()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, waveCollider.radius, revealableLayers);
        
        foreach (var hit in hits)
        {
            HandleRevealable(hit);
        }
    }

    private void OnDestroy()
    {
        DOTween.KillAll();
    }

    private void ScanForAlertables()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, waveCollider.radius, alertableLayers);
        
        foreach (var hit in hits)
        {
            HandleAlertable(hit);
        }
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
    #endregion

    #region Debug Visualization
    private void OnDrawGizmos()
    {
        DrawWaveColliderGizmo();
        DrawTargetRadiusGizmo();
        DrawLightGizmo();
    }

    private void DrawWaveColliderGizmo()
    {
        if (waveCollider == null)
            waveCollider = GetComponent<CircleCollider2D>();

        if (waveCollider == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, waveCollider.radius);

#if UNITY_EDITOR
        var labelPosition = transform.position + Vector3.up * (waveCollider.radius + 0.2f);
        UnityEditor.Handles.Label(labelPosition, $"Wave Radius: {waveCollider.radius:F2}");
#endif
    }

    private void DrawTargetRadiusGizmo()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, targetRadius *0.5f);
    }

    private void DrawLightGizmo()
    {
#if UNITY_EDITOR
        if (waveLight == null || !waveLight.enabled) return;

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(waveLight.transform.position, waveLight.pointLightOuterRadius);

        var labelPosition = waveLight.transform.position + Vector3.up * 0.2f;
        var labelText = $"Light Radius: {waveLight.pointLightOuterRadius:F2} | Intensity: {waveLight.intensity:F2}";
        UnityEditor.Handles.Label(labelPosition, labelText);
#endif
    }
    #endregion
}
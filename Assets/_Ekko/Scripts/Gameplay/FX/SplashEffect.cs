using System;
using UnityEngine;

public class SplashEffect : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private string glowColorProperty = "_GlowColor";
    
    [Header("Pulse Settings")]
    [SerializeField] private float maxGlowIntensity = 3f;
    [SerializeField] private float minGlowIntensity = 0.5f;
    [SerializeField] private float glowingSpeed = 2f;
    [SerializeField] private bool useUnscaledTime = false;
    [SerializeField] private bool isPulsing = true;

    private Renderer targetRenderer;
    private MaterialPropertyBlock propertyBlock;
    private int glowColorPropertyID;
    private Color baseGlowColor;
    private Color startColor;
    private float currentTime = 0f;

    private void Awake()
    {
        SetupMaterialPropertyBlock();
    }

    private void SetupMaterialPropertyBlock()
    {
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            Debug.LogError($"SplashEffect: No Renderer component found on {gameObject.name}");
            return;
        }

        propertyBlock = new MaterialPropertyBlock();
        glowColorPropertyID = Shader.PropertyToID(glowColorProperty);
        
        // Get the current color from the shared material
        if (!targetRenderer.sharedMaterial.HasProperty(glowColorProperty))
        {
            Debug.LogWarning($"SplashEffect: Material doesn't have property '{glowColorProperty}'");
            return;
        }
        
        baseGlowColor = targetRenderer.sharedMaterial.GetColor(glowColorProperty);
        startColor = baseGlowColor;
        
        // Normalize the base color if it has HDR values
        float maxComponent = Mathf.Max(baseGlowColor.r, baseGlowColor.g, baseGlowColor.b);
        if (maxComponent > 1f)
        {
            baseGlowColor = new Color(
                baseGlowColor.r / maxComponent,
                baseGlowColor.g / maxComponent,
                baseGlowColor.b / maxComponent,
                baseGlowColor.a
            );
        }
    }

    private void Update()
    {
        if (!isPulsing || !targetRenderer || propertyBlock == null) return;
        
        // Use either scaled or unscaled time based on preference
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        currentTime += deltaTime * glowingSpeed;
        
        // Create ping-pong effect using PingPong function
        float normalizedIntensity = Mathf.PingPong(currentTime, 1f);
        
        // Lerp between min and max intensity
        float intensity = Mathf.Lerp(minGlowIntensity, maxGlowIntensity, normalizedIntensity);
        
        // Apply intensity to the base color
        Color pulsingColor = baseGlowColor * intensity;
        
        // Use MaterialPropertyBlock to set per-object properties
        propertyBlock.SetColor(glowColorPropertyID, pulsingColor);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnDestroy()
    {
        // Reset the property block to clear any overrides
        if (targetRenderer != null && propertyBlock != null)
        {
            propertyBlock.SetColor(glowColorPropertyID, startColor);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void OnDisable()
    {
        // Reset to original color when disabled
        if (targetRenderer != null && propertyBlock != null)
        {
            propertyBlock.SetColor(glowColorPropertyID, startColor);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
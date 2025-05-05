using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class UIFlickerLamp : MonoBehaviour
{
    [Header("Flicker Settings")]
    [SerializeField] private float minIntensity = 0.4f;
    [SerializeField] private float maxIntensity = 1.0f;
    [SerializeField] private float flickerIntervalMin = 0.05f;
    [SerializeField] private float flickerIntervalMax = 0.2f;

    private TextMeshProUGUI tmpText;
    private Image uiImage;
    private Light2D light2D;
    private Coroutine flickerRoutine;
    private float baseAlpha;
    private float baseIntensity;

    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        uiImage = GetComponent<Image>();
        light2D = GetComponent<Light2D>();

        if (tmpText != null)
            baseAlpha = tmpText.color.a;

        if (uiImage != null)
            baseAlpha = uiImage.color.a;

        if (light2D != null)
            baseIntensity = light2D.intensity;
    }

    private void OnEnable()
    {
        flickerRoutine = StartCoroutine(FlickerLoop());
    }

    private void OnDisable()
    {
        if (flickerRoutine != null)
            StopCoroutine(flickerRoutine);
    }

    private IEnumerator FlickerLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(flickerIntervalMin, flickerIntervalMax);
            float intensity = Random.Range(minIntensity, maxIntensity);

            if (tmpText != null)
            {
                Color c = tmpText.color;
                c.a = baseAlpha * intensity;
                tmpText.color = c;
            }

            if (uiImage != null)
            {
                Color c = uiImage.color;
                c.a = baseAlpha * intensity;
                uiImage.color = c;
            }

            if (light2D != null)
            {
                light2D.intensity = baseIntensity * intensity;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }
}
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LightFlasher : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private float flashSpeed = 1f;
    [SerializeField] private float flashIntensity = 2f;
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private bool lockOnTarget = false; // 👈 détermine si la lumière reste allumée

    private Light2D light2D;
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        if (light2D != null)
            light2D.enabled = false;
    }

    public void StartFlashing()
    {
        if (flashRoutine == null && light2D != null)
        {
            flashRoutine = StartCoroutine(FlashLoop());
        }
    }

    public void StopFlashing()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;

            if (!lockOnTarget)
            {
                light2D.intensity = 0f;
                light2D.enabled = false;
            }
        }
    }

    private IEnumerator FlashLoop()
    {
        light2D.enabled = true;

        while (true)
        {
            float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
            light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            yield return null;
        }
    }

    public void LockOn()
    {
        StopFlashing();
        light2D.enabled = true;
        light2D.intensity = flashIntensity;
    }

    public void Unlock()
    {
        StopFlashing();
        light2D.intensity = 0f;
        light2D.enabled = false;
    }
}
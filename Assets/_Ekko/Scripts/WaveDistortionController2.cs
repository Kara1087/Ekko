using UnityEngine;

public class WaveDistortionController2 : MonoBehaviour
{
    [Header("References")]
    public Material waveMaterial;
    public Camera cam;

    [Header("Wave Settings")]
    public float maxRadius = 1.5f;
    public float duration = 1.0f;
    public float thickness = 0.1f;
    public float strength = 0.05f;
    public float noiseAmount = 0.02f;
    public float noiseScale = 20f;
    public float falloff = 2f;
    public AnimationCurve strengthCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    float _timer;
    bool _playing;
    Vector2 _center;
    float _baseStrength;

    static readonly int WaveCenter = Shader.PropertyToID("_WaveCenter");
    static readonly int WaveRadius = Shader.PropertyToID("_WaveRadius");
    static readonly int WaveThickness = Shader.PropertyToID("_WaveThickness");
    static readonly int WaveStrength = Shader.PropertyToID("_WaveStrength");
    static readonly int WaveNoise = Shader.PropertyToID("_WaveNoise");
    static readonly int WaveNoiseScale = Shader.PropertyToID("_WaveNoiseScale");
    static readonly int WaveFalloff = Shader.PropertyToID("_WaveFalloff");

    void Start()
    {
        if (cam == null) cam = Camera.main;
        ResetWave();
    }

    void Update()
    {
        if (!_playing) return;

        _timer += Time.deltaTime;
        float t = _timer / duration;

        if (t >= 1f)
        {
            ResetWave();
            return;
        }

        float radius = Mathf.Lerp(0, maxRadius, t);
        float str = _baseStrength * strengthCurve.Evaluate(t);

        waveMaterial.SetVector(WaveCenter, _center);
        waveMaterial.SetFloat(WaveRadius, radius);
        waveMaterial.SetFloat(WaveThickness, thickness);
        waveMaterial.SetFloat(WaveStrength, str);
        waveMaterial.SetFloat(WaveNoise, noiseAmount);
        waveMaterial.SetFloat(WaveNoiseScale, noiseScale);
        waveMaterial.SetFloat(WaveFalloff, falloff);
    }

    public void TriggerAtWorldPos(Vector3 worldPos)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);
        TriggerAtScreenUV(new Vector2(vp.x, vp.y));
    }

    public void TriggerAtScreenUV(Vector2 uv)
    {
        _center = uv;
        _baseStrength = strength;
        _timer = 0;
        _playing = true;
    }

    public void TriggerAtScreenCenter()
    {
        TriggerAtScreenUV(new Vector2(0.5f, 0.5f));
    }

    void ResetWave()
    {
        _playing = false;
        waveMaterial.SetFloat(WaveStrength, 0);
        waveMaterial.SetFloat(WaveRadius, 0);
    }
}
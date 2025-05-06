using UnityEngine;

public class LightRevealManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private GameObject wavePrefab;
    [SerializeField] private float waveImpactForce = 30f;
    [SerializeField] private float waveTargetRadius = 35f;

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private bool hasWaveBeenTriggered = false;

    public void StartReveal()
    {
        if (hasWaveBeenTriggered)
        {
            if (debug) Debug.Log("⚠️ Wave déjà lancée, skip.");
            return;
        }

        hasWaveBeenTriggered = true;

        if (wavePrefab == null)
        {
            Debug.LogWarning("❌ Aucun prefab de Wave assigné !");
            return;
        }

        GameObject waveGO = Instantiate(wavePrefab, transform.position, Quaternion.identity);
        if (!waveGO.TryGetComponent(out Wave wave))
        {
            Debug.LogWarning("❌ Le prefab Wave n’a pas de composant Wave !");
            return;
        }

        wave.Initialize(waveImpactForce, waveTargetRadius);

        if (debug) Debug.Log("✅ Wave lancée avec succès.");
    }

    public void ResetReveal()
    {
        hasWaveBeenTriggered = false;
        if (debug) Debug.Log("🔁 Reset du LightRevealManager.");
    }
}
using UnityEngine;

public class LightRevealManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private GameObject wavePrefab;
    [SerializeField] private float baseImpactForce = 30f;
    [SerializeField] private float baseTargetRadius = 35f;
    [SerializeField] private float growthPerBeat = 5f; // 👈 croissance par beat

    [Header("Beat Sync")]
    [SerializeField] private bool useBeatSync = false;
    [SerializeField] private int spawnIntervalInBeats = 2; // 👈 génère 1 wave tous les X beats

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private bool isRevealing = false;
    private int beatCount = 0;

    public void StartReveal()
    {
        if (isRevealing)
        {
            if (debug) Debug.Log("⚠️ Déjà en cours de révélation.");
            return;
        }

        isRevealing = true;
        beatCount = 0;

        if (useBeatSync)
        {
            if (MusicConductor.Instance != null)
            {
                MusicConductor.Instance.OnBeat.AddListener(SpawnWave);
                if (debug) Debug.Log("🎵 Reveal synchronisé sur le beat.");
            }
            else
            {
                Debug.LogWarning("❌ MusicConductor non trouvé !");
            }
        }
        else
        {
            SpawnWave();
        }
    }

    public void ResetReveal()
    {
        if (useBeatSync && isRevealing && MusicConductor.Instance != null)
        {
            MusicConductor.Instance.OnBeat.RemoveListener(SpawnWave);
            if (debug) Debug.Log("⛔ Arrêt de la synchro beat.");
        }

        isRevealing = false;
        beatCount = 0;

        if (debug) Debug.Log("🔁 Reset du LightRevealManager.");
    }

    private void SpawnWave()
    {
        if (!isRevealing) return;

        beatCount++;

        if (beatCount % spawnIntervalInBeats != 0) return; // 👈 on skip le beat

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

        float impactForce = baseImpactForce + (beatCount * growthPerBeat);
        float targetRadius = baseTargetRadius + (beatCount * growthPerBeat);

        wave.Initialize(impactForce, targetRadius);

        if (debug) Debug.Log($"✅ Wave (beat #{beatCount}) — Force: {impactForce}, Radius: {targetRadius}");
    }
}
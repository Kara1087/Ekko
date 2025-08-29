using UnityEngine;

public class LightRevealManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private Wave wavePrefab;
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
    private float revealStartTime;

    public void StartReveal()
    {
        if (isRevealing)
        {
            if (debug) Debug.Log("⚠️ Déjà en cours de révélation.");
            return;
        }

        isRevealing = true;
        beatCount = 0;
        revealStartTime = Time.time;

        if (debug) Debug.Log($"🌀 Reveal démarré à {revealStartTime:0.00}s");

        if (useBeatSync)
        {
            if (MusicConductor.Instance != null)
            {
                MusicConductor.Instance.OnBeat.AddListener(SpawnWave);
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
        }

        isRevealing = false;
        beatCount = 0;
    }

    private void SpawnWave()
    {
        if (!isRevealing) return;

        beatCount++;

        if (beatCount % spawnIntervalInBeats != 0) return; // 👈 on skip le beat

        float impactForce = baseImpactForce + (beatCount * growthPerBeat);
        float targetRadius = baseTargetRadius + (beatCount * growthPerBeat);

        if (debug)
        {
            float timeSinceStart = Time.time - revealStartTime;
            Debug.Log($"🌊 Wave #{beatCount} | Temps : {timeSinceStart:0.00}s | Force : {impactForce} | Rayon : {targetRadius}");
        }

        if (wavePrefab == null)
        {
            Debug.LogWarning("❌ Aucun prefab de Wave assigné !");
            return;
        }

        //Pass in ObjectPool
        Wave wave =  ObjectPoolManager.SpawnObject(wavePrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Wave);
        wave.Initialize(impactForce, targetRadius);
    }
}
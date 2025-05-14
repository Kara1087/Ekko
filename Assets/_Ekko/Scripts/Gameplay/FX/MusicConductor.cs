using UnityEngine;
using UnityEngine.Events;

public class MusicConductor : MonoBehaviour
{
    public static MusicConductor Instance;

    [SerializeField] private AudioSource source;   // 🎧 L’AudioSource à écouter
    [SerializeField] private float bpm = 120f;      // ⚙️ BPM exact de la track
    public UnityEvent OnBeat;

    private float secondsPerBeat;
    private float nextBeatTime;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        secondsPerBeat = 60f / bpm;
        nextBeatTime = 0f;
    }

    private void Update()
    {
        if (source == null || !source.isPlaying) return;

        if (source.time >= nextBeatTime)
        {
            OnBeat?.Invoke();
            nextBeatTime += secondsPerBeat;
        }
    }

    public void SetSource(AudioSource newSource, float newBpm)
    {
        source = newSource;
        bpm = newBpm;
        secondsPerBeat = 60f / bpm;
        nextBeatTime = 0f;
    }
}

using UnityEngine;
using UnityEngine.Events;

public class MusicConductor : MonoBehaviour
{
    // Singleton : permet un accès global à l'instance actuelle du chef d’orchestre musical
    public static MusicConductor Instance;

    [SerializeField] private AudioSource source;    // 🎧 L’AudioSource à écouter
    [SerializeField] private float bpm = 120f;      // ⚙️ BPM exact de la track

    [Header("Événement de beat")]
    public UnityEvent OnBeat;                       // 🔊 Événement déclenché à chaque beat

    private float secondsPerBeat;                   // ⏱️ Durée d’un beat en secondes
    private float nextBeatTime;                     // ⏱️ Temps du prochain beat

    private void Awake()
    {
        // Pattern Singleton : s’assure qu’une seule instance existe dans la scène
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        // Calcule la durée d’un beat selon le BPM
        secondsPerBeat = 60f / bpm;

        // Initialisation du premier beat à 0
        nextBeatTime = 0f;
    }

    private void Update()
    {
        // Ne fait rien si la musique n’est pas en cours de lecture
        if (source == null || !source.isPlaying) return;

        // Vérifie si on a atteint le moment du prochain beat
        if (source.time >= nextBeatTime)
        {
            // 🎯 Déclenche l’événement lié au rythme
            OnBeat?.Invoke();

            // Calcule le temps du prochain beat
            nextBeatTime += secondsPerBeat;
        }
    }

    /// <summary>
    /// Permet de changer dynamiquement la source audio et son BPM (utile si plusieurs morceaux)
    /// </summary>
    public void SetSource(AudioSource newSource, float newBpm)
    {
        source = newSource;
        bpm = newBpm;

        // Recalcule la durée d’un beat
        secondsPerBeat = 60f / bpm;

        // Réinitialise le compteur
        nextBeatTime = 0f;
    }
}

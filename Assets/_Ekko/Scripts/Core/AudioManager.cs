using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AudioManager gère la lecture de musiques, SFX et overlays. Singleton persistant entre les scènes.
/// Interagit avec GameManager et UIManager.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public float bpm = 120f; // 🆕 BPM pour synchronisation
    }

    public List<Sound> sounds = new List<Sound>();

    private Dictionary<string, Sound> soundMap;
    private AudioSource sfxSource;
    private AudioSource musicThemeSource;
    private AudioSource musicOverlaySource;
    private string currentMusicName = "";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Création des AudioSources
        sfxSource = gameObject.AddComponent<AudioSource>();

        musicThemeSource = gameObject.AddComponent<AudioSource>();
        musicThemeSource.loop = true;
        musicThemeSource.playOnAwake = false;

        musicOverlaySource = gameObject.AddComponent<AudioSource>();
        musicOverlaySource.loop = false;
        musicOverlaySource.playOnAwake = false;

        // Création de la map de sons
        soundMap = new Dictionary<string, Sound>();
        foreach (var sound in sounds)
        {
            soundMap[sound.name] = sound;
        }
    }

    // 🔊 SFX
    public void Play(string soundName)
    {
        if (!soundMap.ContainsKey(soundName))
        {
            Debug.LogWarning($"[AudioManager] Sound '{soundName}' not found.");
            return;
        }

        Sound s = soundMap[soundName];
        sfxSource.pitch = s.pitch;
        sfxSource.PlayOneShot(s.clip, s.volume);
    }

    // 🎵 Joue une musique principale en boucle
    public void PlayMusicTheme(string soundName)
    {

        if (!soundMap.ContainsKey(soundName))
        {
            Debug.LogWarning($"[AudioManager] ⚠️ Sound '{soundName}' non trouvé dans soundMap.");
            return;
        }

        if (currentMusicName == soundName && musicThemeSource.isPlaying)
        {
            Debug.Log($"[AudioManager] ⏩ Musique déjà en cours : {soundName}");
            return;
        }

        Sound s = soundMap[soundName];
        musicThemeSource.clip = s.clip;
        musicThemeSource.volume = s.volume;
        musicThemeSource.pitch = s.pitch;
        musicThemeSource.Play();

        currentMusicName = soundName;

        // Debug.Log($"[AudioManager] ✅ Lecture de : {soundName}");
    }

    public void StopMusicTheme()
    {
        musicThemeSource.Stop();
    }

    // 🎚️ Volume : Change dynamiquement le volume d'une musique donnée
    public void SetVolume(string soundName, float newVolume)
    {
        if (soundName == currentMusicName && musicThemeSource != null)
        {
            musicThemeSource.volume = newVolume;
        }
    }

    // 🎭 Specific themes
    public void PlayStartTheme() // Appelée depuis le menu principal
    {
        PlayMusicTheme("StartScreenTheme");
    }

    public void PlayGameOverTheme() // Appelée quand le joueur meurt
    {
        PlayMusicTheme("GameOverTheme");
    }

    public void PlayPauseTheme()  // Appelée quand on met le jeu en pause
    {
        PlayMusicTheme("PauseTheme");
    }

    public void StopTheme()  // Appelée pour stopper toute musique
    {
        StopMusicTheme();
    }

    public void PlayOverlayMusic(string soundName)  // 🎶 Overlay : Musique temporaire (superposée à la musique principale)
    {
        if (!soundMap.ContainsKey(soundName)) return;

        Sound s = soundMap[soundName];
        musicOverlaySource.clip = s.clip;
        musicOverlaySource.volume = s.volume;
        musicOverlaySource.pitch = s.pitch;
        musicOverlaySource.Play();

        // 🆕 Synchronisation automatique du MusicConductor
        if (MusicConductor.Instance != null)
        {
            MusicConductor.Instance.SetSource(musicOverlaySource, s.bpm);
        }
    }

    public void StopOverlayMusic()
    {
        if (musicOverlaySource.isPlaying)
        {
            musicOverlaySource.Stop();
        }
    }
}
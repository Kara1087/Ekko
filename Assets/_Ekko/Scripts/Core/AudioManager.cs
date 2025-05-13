using UnityEngine;
using System.Collections.Generic;

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

        // Sources
        sfxSource = gameObject.AddComponent<AudioSource>();

        musicThemeSource = gameObject.AddComponent<AudioSource>();
        musicThemeSource.loop = true;
        musicThemeSource.playOnAwake = false;

        musicOverlaySource = gameObject.AddComponent<AudioSource>();
        musicOverlaySource.loop = false;
        musicOverlaySource.playOnAwake = false;

        // Sound map
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

    // 🎵 Music Theme
    public void PlayMusicTheme(string soundName)
    {
        if (!soundMap.ContainsKey(soundName)) return;

        if (currentMusicName == soundName && musicThemeSource.isPlaying)
            return;

        Sound s = soundMap[soundName];
        musicThemeSource.clip = s.clip;
        musicThemeSource.volume = s.volume;
        musicThemeSource.pitch = s.pitch;
        musicThemeSource.Play();

        currentMusicName = soundName;
    }

    public void StopMusicTheme()
    {
        musicThemeSource.Stop();
    }

    // 🎚️ Volume
    public void SetVolume(string soundName, float newVolume)
    {
        if (soundName == currentMusicName && musicThemeSource != null)
        {
            musicThemeSource.volume = newVolume;
        }
    }

    // 🎭 UI-specific themes
    public void SetMusicForScreen(UIScreen screen)
    {
        switch (screen)
        {
            case UIScreen.Start:
                PlayMusicTheme("StartScreenTheme");
                break;
            case UIScreen.GameOver:
            case UIScreen.Pause:
            case UIScreen.None:
                StopMusicTheme();
                break;
        }
    }

    // 🎶 Overlay
    public void PlayOverlayMusic(string soundName)
    {
        if (!soundMap.ContainsKey(soundName)) return;

        Sound s = soundMap[soundName];
        musicOverlaySource.clip = s.clip;
        musicOverlaySource.volume = s.volume;
        musicOverlaySource.pitch = s.pitch;
        musicOverlaySource.Play();

        // 🆕 Synchronisation automatique du MusicConductor avec cette musique
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
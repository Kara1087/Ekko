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
    }

    public List<Sound> sounds = new List<Sound>();

    private Dictionary<string, Sound> soundMap;
    private AudioSource sfxSource;
    private AudioSource musicThemeSource;
    private AudioSource musicOverlaySource;
    private string currentMusicName = "";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSources
        sfxSource = gameObject.AddComponent<AudioSource>();
        
        musicThemeSource = gameObject.AddComponent<AudioSource>();
        musicThemeSource.loop = true;
        musicThemeSource.playOnAwake = false;

        musicOverlaySource = gameObject.AddComponent<AudioSource>();
        musicOverlaySource.loop = false;
        musicOverlaySource.playOnAwake = false;

        // Mapping sons
        soundMap = new Dictionary<string, Sound>();
        foreach (var sound in sounds)
        {
            soundMap[sound.name] = sound;
        }
    }

    public void Play(string soundName)
    {
        if (!soundMap.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
            return;
        }

        Sound s = soundMap[soundName];
        sfxSource.pitch = s.pitch;
        sfxSource.PlayOneShot(s.clip, s.volume);
    }

    public void PlayMusicTheme(string soundName)
    {
        if (!soundMap.ContainsKey(soundName))
        {
            Debug.LogWarning($"[AudioManager] Music theme '{soundName}' not found!");
            return;
        }

        if (musicThemeSource.isPlaying && currentMusicName != soundName)
        {
            musicThemeSource.Stop(); // On arrête l’ancienne musique si elle est différente
        }

        Sound s = soundMap[soundName];
        musicThemeSource.clip = s.clip;
        musicThemeSource.volume = s.volume;
        musicThemeSource.pitch = s.pitch;
        musicThemeSource.Play();

        currentMusicName = soundName;
        Debug.Log($"[AudioManager] Now playing: '{currentMusicName}'");
    }

    public void StopMusicTheme()
    {
        musicThemeSource.Stop();
    }

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

    public void PlayOverlayMusic(string soundName)
    {
        if (!soundMap.ContainsKey(soundName))
        {
            Debug.LogWarning($"[AudioManager] Overlay music '{soundName}' not found!");
            return;
        }

        Sound s = soundMap[soundName];
        musicOverlaySource.clip = s.clip;
        musicOverlaySource.volume = s.volume;
        musicOverlaySource.pitch = s.pitch;
        musicOverlaySource.Play();

        Debug.Log($"[AudioManager] Overlay playing: '{soundName}'");
    }

    public void SetVolume(string soundName, float newVolume)
    {
        if (soundName == currentMusicName && musicThemeSource != null)
            musicThemeSource.volume = newVolume;
    }

}
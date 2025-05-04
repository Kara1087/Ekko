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

        // Mapping sons
        soundMap = new Dictionary<string, Sound>();
        foreach (var sound in sounds)
        {
            soundMap[sound.name] = sound;
        }
    }
    void Start()
    {
        PlayMusicTheme("BackgroundTheme");
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
            Debug.LogWarning($"Music theme '{soundName}' not found!");
            return;
        }

        Sound s = soundMap[soundName];
        musicThemeSource.clip = s.clip;
        musicThemeSource.volume = s.volume;
        musicThemeSource.pitch = s.pitch;
        musicThemeSource.Play();
    }

    public void StopMusicTheme()
    {
        musicThemeSource.Stop();
    }
}
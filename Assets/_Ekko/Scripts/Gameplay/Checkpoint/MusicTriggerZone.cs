using UnityEngine;

public class MusicTriggerZone : MonoBehaviour
{
    [Header("Paramètres")]
    [SerializeField] private string musicThemeName = "TensionTheme";
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private bool onlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;

        AudioManager.Instance?.SwitchMusicTheme(musicThemeName, fadeDuration);

        if (onlyOnce)
            Destroy(this);
    }
}
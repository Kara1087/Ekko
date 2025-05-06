using UnityEngine;

public enum UIScreen { None, Start, Pause, GameOver }

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Screens")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private BlackoutEffect blackoutEffect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Lancement du fade-in, puis affichage de l’écran de démarrage
        blackoutEffect?.StartFadeIn(() =>
        {
            ShowScreen(UIScreen.Start);
        });
    }

    public void ShowScreen(UIScreen screen)
    {
        Debug.Log($"[UIManager] 📺 Affichage de l’écran : {screen}");

        startScreen?.SetActive(screen == UIScreen.Start);
        pauseScreen?.SetActive(screen == UIScreen.Pause);
        gameOverScreen?.SetActive(screen == UIScreen.GameOver);

        AudioManager.Instance.SetMusicForScreen(screen); // 🔁 Appel centralisé
        
    }

    public void HideAllScreens()
    {
        startScreen?.SetActive(false);
        pauseScreen?.SetActive(false);
        gameOverScreen?.SetActive(false);
    }

    public void SetPauseScreen(bool show)
    {
        pauseScreen?.SetActive(show);
    }

    public void StartBlackoutEffect()
    {
        blackoutEffect?.StartBlackout();
    }

    // ---------- Boutons UI ----------

    public void OnStartButton()
    {
        HideAllScreens();
        Time.timeScale = 1f;
        // LevelController.Instance.FadeAndLoadScene("Level_1");
        AudioManager.Instance.PlayMusicTheme("BackgroundTheme");
    }

    public void OnExitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnResumeButton()
    {
        GameManager.Instance?.ResumeGame();
    }

    public void OnRetryButton()
    {
        HideAllScreens();
        Time.timeScale = 1f;
        // LevelController.Instance.RestartLevel();
    }

    public void OnMainMenuButton()
    {
        ShowScreen(UIScreen.Start);
        Time.timeScale = 0f;
    }

    public void OnPauseButton()
    {
        GameManager.Instance?.TogglePause();
    }
}
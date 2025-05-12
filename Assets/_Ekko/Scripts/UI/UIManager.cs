using UnityEngine;
using UnityEngine.SceneManagement;

public enum UIScreen { None, Start, Pause, GameOver }

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Screens")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameOverQuotePanel;
    [SerializeField] private BlackoutEffect blackoutEffect;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject); ❌ supprimé pour éviter persistance entre scènes
        
        // Affichage de l'écran de démarrage uniquement dans la scène du menu principal
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            blackoutEffect?.StartFadeIn(() => ShowScreen(UIScreen.Start));
        }
        else
        {
            ShowScreen(UIScreen.None);
        }
    }

    public void ShowScreen(UIScreen screen)
    {

        startPanel?.SetActive(screen == UIScreen.Start);
        pausePanel?.SetActive(screen == UIScreen.Pause);
        gameOverPanel?.SetActive(screen == UIScreen.GameOver);

        AudioManager.Instance.SetMusicForScreen(screen); // 🔁 Appel centralisé
        
    }

    public void HideAllScreens()
    {
        startPanel?.SetActive(false);
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
    }

    public void SetPauseScreen(bool show)
    {
        pausePanel?.SetActive(show);
    }



    public void StartBlackoutEffect()
    {
        blackoutEffect?.StartBlackout();
    }

    // ---------- Boutons UI ----------

    public void OnStartButton()
    {
        GameManager.Instance?.StartGame();
    }

    public void OnExitButton()
    {
        GameManager.Instance?.QuitGame();
    }

    public void OnResumeButton()
    {
        GameManager.Instance?.ResumeGame();
    }

    public void OnRetryButton()
    {
        GameManager.Instance?.RestartGame();
    }

    public void OnMainMenuButton()
    {
        GameManager.Instance?.ReturnToMainMenu();
    }

    public void OnPauseButton()
    {
        GameManager.Instance?.TogglePause();
    }
}
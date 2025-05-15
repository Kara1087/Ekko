using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

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
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        InitSingleton();
        InitUIScreen();
    }

    private void Start()
    {
        if (restartButton != null)
        restartButton.onClick.AddListener(GameManager.Instance.RestartGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(GameManager.Instance.QuitGame);
    }
    private void InitSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void InitUIScreen()
    {
        // Affichage de l'écran de démarrage uniquement dans la scène du menu principal
        if (SceneManager.GetActiveScene().name == "_MainMenu")
        {
            // ✅ Lancer la musique AVANT le fade
            AudioManager.Instance?.SetMusicForScreen(UIScreen.Start);

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
        Debug.Log("[UIManager] ❌ HideAllScreens() appelé");
        
        startPanel?.SetActive(false);
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
    }

    public void SetPauseScreen(bool show)
    {
        pausePanel?.SetActive(show);
    }

    public IEnumerator StartBlackoutRoutine()
    {
        bool finished = false;
        blackoutEffect?.StartBlackout(() => finished = true);
        yield return new WaitUntil(() => finished);
    }

    public IEnumerator StartFadeInRoutine()
    {
        bool finished = false;
        blackoutEffect?.StartFadeIn(() => finished = true);
        yield return new WaitUntil(() => finished);
    }

    private void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(GameManager.Instance.RestartGame);
        if (exitButton != null)
            exitButton.onClick.RemoveListener(GameManager.Instance.QuitGame);
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
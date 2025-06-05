using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Global panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject quotePanel;

    [Header("Blackout Effect")]
    [SerializeField] private BlackoutEffect blackoutEffect;

    [Header("UI Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    

    private void Awake()
    {
        // Singleton pattern : assure qu'un seul UIManager existe
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (blackoutEffect == null)
        blackoutEffect = GetComponentInChildren<BlackoutEffect>(true);

        if (blackoutEffect == null)
            Debug.LogWarning("[UIManager] ❌ BlackoutEffect toujours null après recherche automatique !");
    }

    private void Start()
    {
        // Lancer un fade-in automatique seulement la première fois 👉 automatiquement dans UIBlackoutEffect
        //if (!hasFadedInOnce)
        //{
        //    blackoutEffect?.StartFadeIn(() => hasFadedInOnce = true);
        //}

        // Abonnement aux boutons globaux
        if (restartButton != null)
            restartButton.onClick.AddListener(GameManager.Instance.RestartGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(GameManager.Instance.QuitGame);
    }

    /*private void InitUIScreen()
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
    }*/

    /*public void ShowScreen(UIScreen screen)
    {
        //startPanel?.SetActive(screen == UIScreen.Start);
        pausePanel?.SetActive(screen == UIScreen.Pause);
        gameOverPanel?.SetActive(screen == UIScreen.GameOver);

        AudioManager.Instance.SetMusicForScreen(screen); // 🔁 Appel centralisé
    }

    public void HideAllScreens()
    {
        //Debug.Log("[UIManager] ❌ HideAllScreens() appelé");
        
        //startPanel?.SetActive(false);
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
    }

    public void SetPauseScreen(bool show)
    {
        if (pausePanel != null)
            pausePanel?.SetActive(show);
    }*/

    // Affiche ou cache le panel de pause
    public void ShowPause(bool show)
    {
        if (pausePanel != null)
            pausePanel.SetActive(show);
            
        if (show)
            AudioManager.Instance?.PlayPauseTheme();
        else
            AudioManager.Instance?.PlayMusicTheme("BackgroundTheme");
    }

    // Active l'écran de Game Over
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // Cache manuellement l'écran de Game Over
    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // Affiche ou cache le HUD
    public void ShowQuotePanel(bool show)
    {
        if (quotePanel != null)
            quotePanel.SetActive(show);
    }

    // Lance un blackout (fondu vers noir) et attend la fin de l'effet
    public IEnumerator StartBlackoutRoutine()
    {
        bool finished = false;

        blackoutEffect?.StartBlackout(() => finished = true);

        yield return new WaitUntil(() => finished);
        //onComplete?.Invoke();
    }

    public IEnumerator StartFadeInRoutine()
    {
        bool finished = false;
        if (blackoutEffect != null)
        blackoutEffect.StartFadeIn(() => finished = true);
        else
        {
            Debug.LogWarning("[UIManager] ⚠️ Pas de blackoutEffect → fade ignoré");
            finished = true; // force la poursuite
        }
        yield return new WaitUntil(() => finished);
        //onComplete?.Invoke();
    }

    // Nettoie les listeners au moment de la destruction
    private void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(GameManager.Instance.RestartGame);
        if (exitButton != null)
            exitButton.onClick.RemoveListener(GameManager.Instance.QuitGame);
    }

    // ---------- Boutons UI ----------

    /*public void OnStartButton()
    {
        GameManager.Instance?.StartGame();
    }*/

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
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsPaused { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;

    private QuoteManager quoteManager;
    private BlackoutEffect blackoutEffect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); ❌ supprimé pour éviter persistance entre scènes
    }

    private void Start()
    {
        quoteManager = FindObjectOfType<QuoteManager>();
        blackoutEffect = FindObjectOfType<BlackoutEffect>();

        if (quoteManager == null)
            Debug.LogWarning("❌ QuoteManager non trouvé dans la scène.");
        if (blackoutEffect == null)
            Debug.LogWarning("❌ BlackoutEffect non trouvé dans la scène.");
    }
    
    private void Update()
    {
        if (!IsGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        UIManager.Instance?.SetPauseScreen(IsPaused);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        UIManager.Instance?.SetPauseScreen(false);
    }

    public void HandlePlayerDeath()
    {
        Debug.Log("[GameManager] 💀 Player is dead.");
        IsGameOver = true;
        Time.timeScale = 0f;

        if (quoteManager != null)
        {
            quoteManager.ShowRandomQuote(() =>
            {
                ShowGameOverSequence();
            });
        }
        else
        {
            ShowGameOverSequence(); // fallback direct
        }
    }

    private void ShowGameOverSequence()
    {
        if (blackoutEffect != null)
        {
            blackoutEffect.StartBlackout(); // Appelle ShowScreen(GameOver) en interne
        }
        else
        {
            Debug.LogWarning("❌ Aucun BlackoutEffect disponible.");
            UIManager.Instance?.ShowScreen(UIScreen.GameOver);
        }
    }

    public void StartGame()
    {
        Debug.Log("[GameManager] ▶️ StartGame()");
        
        Time.timeScale = 1f;
        IsPaused = false;
        IsGameOver = false;

        SceneManager.LoadScene("Level_1");   // Charge la scène de jeu

        AudioManager.Instance.PlayMusicTheme("BackgroundTheme");
    }

    public void RestartGame()
    {
        Debug.Log("[GameManager] 🔁 Restart current level");

        Time.timeScale = 1f;
        IsPaused = false;
        IsGameOver = false;

        UIManager.Instance?.HideAllScreens();

        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    public void QuitGame()
    {
        Debug.Log("[GameManager] 🚪 Quit Game");

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("[GameManager] 🏠 Return to Main Menu");

        Time.timeScale = 1f;
        IsPaused = false;
        IsGameOver = false;

        SceneManager.LoadScene("_MainMenu");
    }

}
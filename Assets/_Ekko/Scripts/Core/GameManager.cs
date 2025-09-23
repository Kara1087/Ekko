using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager gère l'état global du jeu : pause, game over, transitions, etc.
/// Il est persistant entre les scènes et interagit avec UIManager.
/// </summary>
/// 
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool IsPaused { get; set; } = false;
    public bool IsGameOver { get; private set; } = false;

    private QuoteData cushionOverrideDeathQuote = null;
    private QuoteManager quoteManager;
    private BlackoutEffect blackoutEffect;
    private const string MAIN_MENU_SCENE = "_MainMenu"; // Cache string constant


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        quoteManager = FindFirstObjectByType<QuoteManager>();
        blackoutEffect = FindFirstObjectByType<BlackoutEffect>();
    }

    private void Start()
    {
        // Lancer le menu principal si on démarre depuis _Bootstrap
        StartMainMenuTransition();
    }
    
    private void StartMainMenuTransition()
    {
        StartCoroutine(TransitionManager.Instance.LoadSceneWithFade(MAIN_MENU_SCENE));
    }

    public void TogglePause()
    {
        if (IsGameOver) return;    
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        UIManager.Instance.ShowPause(IsPaused);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        UIManager.Instance?.ShowPause(false);

        if (IsPaused)
            AudioManager.Instance.PlayPauseTheme();
        else
            AudioManager.Instance.PlayMusicTheme("BackgroundTheme");
    }

    //TODO Why Quota is managed by GameManager
    public void MarkNextDeathAsCushionOnboarding(QuoteData quote)
    {
        cushionOverrideDeathQuote = quote;
    }

    public bool HasOverrideDeathQuote()
    {
        return cushionOverrideDeathQuote;
    }

    public QuoteData GetOverrideDeathQuote()
    {
        return cushionOverrideDeathQuote;
    }

    public void ClearOverrideDeathQuote()
    {
        cushionOverrideDeathQuote = null;
    }

    public void HandlePlayerDeath()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        Time.timeScale = 0f;        // Stop le temps et les inputs ? TODO check

        AudioManager.Instance.FadeOutMusicTheme(2f);
        TransitionManager.Instance.PlayDeathSequence();
    }

    
    public void TryRespawnPlayer()
    {
        // Récupère la positions du dernier checkpoint
        if (!CheckpointManager.Instance || !CheckpointManager.Instance.HasCheckpoint())
        {
            RestartGame();
            return;
        }
        // Réactive le jeu
        Time.timeScale = 1f;
        IsPaused = false;
        IsGameOver = false;
        
        EventManager.TriggerEvent(GameEventType.PlayerRespawn);
        
        // UI : Nettoie le GameOver
        //UIManager.Instance?.HideGameOver();
        UIManager.Instance.ShowQuotePanel(false);

        Debug.Log("[GameManager] ✅ Respawn effectué depuis le dernier checkpoint.");
    }


    public void StartGame(UIMainMenu uiMainMenu)
    {
        Debug.Log("[GameManager] ▶️ StartGame()");

        Time.timeScale = 1f;
        IsPaused = false;
        IsGameOver = false;

        //UIManager.Instance?.ShowQuotePanel(true);
        //UIManager.Instance?.HideGameOver();

        TransitionManager.Instance.PlayIntroSequence(uiMainMenu);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        IsGameOver = false;

        //UIManager.Instance?.ShowQuotePanel(true); TODO demander a Karine pourquoi ?
        UIManager.Instance?.HideGameOver();

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

        UIManager.Instance?.ShowQuotePanel(false);
        UIManager.Instance?.HideGameOver();

        StartCoroutine(TransitionManager.Instance.LoadSceneWithFade("_MainMenu"));
    }
    
    private void EnsureDependencies() // Recherche manuelle des composants si absents
    {
        if (quoteManager == null)
            quoteManager = FindFirstObjectByType<QuoteManager>();

        if (blackoutEffect == null)
        {
            GameObject blackoutGO = GameObject.Find("UI_BlackoutPanel");

            if (blackoutGO != null)
            {
                // 👇 S'il est inactif, on l'active temporairement pour récupérer le script
                bool wasInactive = !blackoutGO.activeSelf;
                if (wasInactive) blackoutGO.SetActive(true);

                blackoutEffect = blackoutGO.GetComponent<BlackoutEffect>();

                if (wasInactive) blackoutGO.SetActive(false); // 👈 On le remet dans son état initial
            }

            /*if (blackoutEffect == null)
            {
                Debug.LogWarning("❌ BlackoutEffect définitivement introuvable.");
            }*/
        }
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log($"[GameManager] 🔄 Scene '{scene.name}' loaded. Réinitialisation des dépendances...");
        EnsureDependencies();
    }

}
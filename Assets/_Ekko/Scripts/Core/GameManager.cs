using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
        DontDestroyOnLoad(gameObject);

        quoteManager = FindFirstObjectByType<QuoteManager>();
        blackoutEffect = FindFirstObjectByType<BlackoutEffect>();

        if (quoteManager == null)
            Debug.LogWarning("❌ QuoteManager non trouvé dans la scène.");
        if (blackoutEffect == null)
            Debug.LogWarning("❌ BlackoutEffect non trouvé dans la scène.");
    }

    private void Start()
    {
        Debug.Log($"[Debug] Time.timeScale = {Time.timeScale}");
        Time.timeScale = 1f;
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
        if (IsGameOver) return;

        Debug.Log("[GameManager] 💀 Player is dead.");
        IsGameOver = true;
        Time.timeScale = 0f;

        EnsureDependencies(); 
        if (quoteManager != null)
        {
            quoteManager.ShowRandomQuote(OnQuoteComplete);
        }
        else
        {
            Debug.LogWarning("❌ QuoteManager non trouvé.");
            OnQuoteComplete();
        }
    }

    private void OnQuoteComplete()
    {
        // 🔄 Forcer une resynchronisation à chaud
        blackoutEffect = FindFirstObjectByType<BlackoutEffect>();

        if (blackoutEffect == null)
        {
            Debug.LogWarning("❌ BlackoutEffect non trouvé. Tentative via UIManager...");
            blackoutEffect = UIManager.Instance?.GetComponentInChildren<BlackoutEffect>(true);
        }

        if (blackoutEffect != null)
        {
            blackoutEffect.StartBlackout(() =>
            {
                Debug.Log("✅ Affichage GameOver après blackout");
                UIManager.Instance?.ShowScreen(UIScreen.GameOver);
            });
        }
        else
        {
            Debug.LogWarning("❌ BlackoutEffect définitivement introuvable. Affichage direct GameOver.");
            UIManager.Instance?.ShowScreen(UIScreen.GameOver);
        }
    }

    public void StartGame()
    {
        Debug.Log("[GameManager] ▶️ StartGame()");
        
        Time.timeScale = 1f;
        IsPaused = false;
        IsGameOver = false;

        UIManager.Instance?.HideAllScreens(); // 🔁 Appelé une fois le bon UIManager chargé
        
        AudioManager.Instance.PlayMusicTheme("BackgroundTheme");

        StartCoroutine(LoadLevelRoutine("Level_1"));
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

    private void EnsureDependencies()
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

            if (blackoutEffect == null)
            {
                Debug.LogWarning("❌ BlackoutEffect définitivement introuvable.");
            }
        }
    }

    private IEnumerator LoadLevelRoutine(string sceneName)
    {
        yield return new WaitForSecondsRealtime(0.1f); // laisse le temps à Unity de désactiver l’UI
        SceneManager.LoadScene(sceneName);
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
        Debug.Log($"[GameManager] 🔄 Scene '{scene.name}' loaded. Réinitialisation des dépendances...");
        EnsureDependencies();
    }

}
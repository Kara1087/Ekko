using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// GameManager gère l'état global du jeu : pause, game over, transitions, etc.
/// Il est persistant entre les scènes et interagit avec UIManager.
/// </summary>
/// 
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
    }

    private void Start()
    {
        // Commenter pour phase test
        // Lancer le menu principal si on démarre depuis _Bootstrap
        ///if (SceneManager.GetActiveScene().name == "_Bootstrap")
        ///{
        ///    SceneLoader.Instance.LoadSceneWithFade("_MainMenu");
        ///}
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
        UIManager.Instance?.ShowPause(IsPaused);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        UIManager.Instance?.ShowPause(false);

        if (IsPaused)
            AudioManager.Instance?.PlayPauseTheme();
        else
            AudioManager.Instance?.PlayMusicTheme("BackgroundTheme");
    }

    public void HandlePlayerDeath()
    {
        Debug.Log("[GameManager] ☠️ HandlePlayerDeath()");
        if (IsGameOver) return;

        Debug.Log("[GameManager] 💀 Player is dead.");
        IsGameOver = true;
        Time.timeScale = 0f;

        EnsureDependencies();
        if (quoteManager != null)
        {
            quoteManager.ShowRandomQuote(QuoteType.Death, OnQuoteComplete);
        }
        else
        {
            Debug.LogWarning("❌ QuoteManager non trouvé.");
            OnQuoteComplete();
        }

        //AudioManager.Instance?.PlayGameOverTheme();
    }

    private void OnQuoteComplete()
    {
        // Rechercher le blackout effect au besoin
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
                UIManager.Instance?.ShowGameOver();
            });
        }
        else
        {
            Debug.LogWarning("❌ BlackoutEffect définitivement introuvable. Affichage direct GameOver.");
            UIManager.Instance?.ShowGameOver();
        }
    }

    public void RespawnPlayer()
    {
        Debug.Log("[GameManager] 🌌 RespawnPlayer()");

        // Récupère la positions du dernier checkpoint
        if (!CheckpointManager.Instance || !CheckpointManager.Instance.HasCheckpoint())
        {
            Debug.LogWarning("[GameManager] Aucun checkpoint trouvé, rechargement de la scène...");
            RestartGame();
            return;
        }

        Vector2 checkpointPos = CheckpointManager.Instance.GetLastCheckpointPosition();

        // Réactive le jeu
        Time.timeScale = 1f;
        IsPaused = false;
        IsGameOver = false;

        // Reset du joueur
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Replace le joueur au checkpoint
            player.transform.position = checkpointPos;

            // Réinitialise la vie/lumière
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.SetLight(ph.MaxLight); // Réinitialise la vie/lumière
            }

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        // UI : Nettoie le GameOver
        UIManager.Instance?.HideGameOver();
        UIManager.Instance?.ShowQuotePanel(false);

        Debug.Log("[GameManager] ✅ Respawn effectué depuis le dernier checkpoint.");
    }


    public void StartGame()
    {
        Debug.Log("[GameManager] ▶️ StartGame()");

        Time.timeScale = 1f;
        IsPaused = false;
        IsGameOver = false;

        UIManager.Instance?.ShowQuotePanel(true);
        UIManager.Instance?.HideGameOver();

        quoteManager.ShowRandomQuote(QuoteType.Intro, () =>
        {
            Debug.Log("[GameManager] 🎬 Intro quote terminée, on charge la scène");

            // ⛔ Désactiver le main menu AVANT le fade vers noir
            var mainMenu = FindAnyObjectByType<UIMainMenu>();
            if (mainMenu != null) mainMenu.Hide();

            // 🎵 musique
            AudioManager.Instance?.StopTheme();  // Arrêt de la musique de menu

            // 🎮 scène
            SceneLoader.Instance.LoadSceneWithFade("Level_1");
        });
    }

    public void RestartGame()
    {
        Debug.Log("[GameManager] 🔁 Restart current level");

        Time.timeScale = 1f;
        IsPaused = false;
        IsGameOver = false;

        UIManager.Instance?.ShowQuotePanel(true);
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

        SceneLoader.Instance.LoadSceneWithFade("_MainMenu");
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
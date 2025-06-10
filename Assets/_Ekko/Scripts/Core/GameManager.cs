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

    private bool startGame; // Pour éviter de lancer le jeu avant que tout soit prêt
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
        startGame = true;
    }

    private void Update()
    {
        if (startGame && SceneManager.GetActiveScene().name == "_Bootstrap")
        {
            startGame = false;
            StartCoroutine(TransitionManager.Instance.LoadSceneWithFade("_MainMenu"));
            return;
        }

        if (!IsGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
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

    public void HandlePlayerDeath()
    {
        if (IsGameOver) return;

        Debug.Log("[GameManager] 💀 Player is dead.");
        IsGameOver = true;
        Time.timeScale = 0f;        // Stop le temps et les inputs

        AudioManager.Instance?.FadeOutMusicTheme(2f);
        TransitionManager.Instance.PlayDeathSequence();
        
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
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // Trouve le joueur dynamiquement (au cas où il a été détruit ou désactivé)
        // Sécurité : on détache le joueur de toute plateforme potentielle
        if (player.transform.parent != null)
        {
            Debug.LogWarning("[GameManager] 🔧 Reset parent du joueur lors du respawn");
            player.transform.SetParent(null);
        }

        if (player != null)
        {
            // Replace le joueur au checkpoint
            player.transform.position = checkpointPos;

            // Réinitialise EnemeyAI
            ResetAllEnemies();

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
        //UIManager.Instance?.HideGameOver();
        UIManager.Instance.ShowQuotePanel(false);

        Debug.Log("[GameManager] ✅ Respawn effectué depuis le dernier checkpoint.");
    }

    public void ResetAllEnemies()
    {
        foreach (var enemy in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
        {
            enemy.ResetToCheckpoint();
        }
    }


    public void StartGame()
    {
        Debug.Log("[GameManager] ▶️ StartGame()");

        Time.timeScale = 1f;
        IsPaused = false;
        IsGameOver = false;

        //UIManager.Instance?.ShowQuotePanel(true);
        //UIManager.Instance?.HideGameOver();

        TransitionManager.Instance.PlayIntroSequence();
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
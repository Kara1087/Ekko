using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsPaused { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        Time.timeScale = 0f;        // Important pour figer le jeu
        
        // Lance le blackout qui s’occupe de la suite (GameOver screen)
        BlackoutEffect blackout = FindFirstObjectByType<BlackoutEffect>();
        if (blackout != null)
        {
            blackout.StartBlackout();
        }
        else
        {
            Debug.LogWarning("[GameManager] Aucun BlackoutEffect trouvé.");
            UIManager.Instance?.ShowScreen(UIScreen.GameOver); // fallback
        }
    }

    public void RestartLevel()
    {
        Debug.Log("[GameManager] 🔄 Restarting level...");
        Time.timeScale = 1f; // Remettre le temps normal au cas où
        IsPaused = false;
        IsGameOver = false;
        
        UIManager.Instance?.HideAllScreens(); // Cacher tous les écrans

        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
}
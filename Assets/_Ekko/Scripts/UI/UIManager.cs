using UnityEngine;

public enum UIScreen { None, Start, Pause, GameOver }

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Screens")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject gameOverScreen;

    private UIScreen currentScreen = UIScreen.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ShowScreen(UIScreen.Start); // écran de lancement
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void ShowScreen(UIScreen screen)
    {
        currentScreen = screen;

        startScreen.SetActive(screen == UIScreen.Start);
        pauseScreen.SetActive(screen == UIScreen.Pause);
        gameOverScreen.SetActive(screen == UIScreen.GameOver);

        if (screen == UIScreen.Pause)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    public void HideAllScreens()
    {
        startScreen.SetActive(false);
        pauseScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        currentScreen = UIScreen.None;
        Time.timeScale = 1f;
    }

    private void TogglePause()
    {
        if (currentScreen == UIScreen.Pause)
        {
            ShowScreen(UIScreen.None);
        }
        else if (currentScreen == UIScreen.None)
        {
            ShowScreen(UIScreen.Pause);
        }
    }

    // 🎮 Boutons UI
    public void OnStartButton()
    {
        HideAllScreens();
        // LevelController.Instance.FadeAndLoadScene("Level_1");
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
        ShowScreen(UIScreen.None);
    }

    public void OnRetryButton()
    {
        HideAllScreens();
        // LevelController.Instance.RestartLevel();
    }

    public void OnMainMenuButton()
    {
        ShowScreen(UIScreen.Start);
    }
}
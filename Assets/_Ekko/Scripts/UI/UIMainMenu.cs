using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// UIManagerLocal gère l'interface spécifique à la scène (ex: menu principal).
/// Contrairement à UIManager (global), il n'est pas persistant.
/// </summary>
public class UIMainMenu : MonoBehaviour
{
    [Header("UI Panels locaux")]
    [SerializeField] private GameObject mainMenuPanel;

    [Header("UI Boutons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        // Activation par défaut du menu principal
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    private void Start()
    {
        // Abonnement aux boutons
        if (playButton != null)
            playButton.onClick.AddListener(GameManager.Instance.StartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(GameManager.Instance.QuitGame);

        // Lancer la musique de menu
        AudioManager.Instance?.PlayStartTheme();
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(GameManager.Instance.StartGame);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(GameManager.Instance.QuitGame);
    }

        public void Hide()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
    }
}
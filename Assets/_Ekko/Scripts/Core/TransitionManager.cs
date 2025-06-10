using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Centralise les séquences de transition du jeu : mort, intro, téléportation, changement de scène, etc.
/// Il enchaîne proprement les citations, fades, respawn et chargements de scènes.
/// </summary>

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    private UIManager ui;
    private GameManager game;
    private QuoteManager quote;
    private bool isRunning = false;

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

    private void Start()
    {
        // Sécurité pour re-capturer les singletons s'ils pas encore prêts à l'Awake
        if (game == null) game = GameManager.Instance;
        if (ui == null) ui = UIManager.Instance;
        if (quote == null) quote = FindFirstObjectByType<QuoteManager>();
    }

    /// <summary>
    /// Lance la séquence de mort : blackout → citation → respawn → fade-in.
    /// </summary>
    public void PlayDeathSequence()
    {
        if (isRunning)
        {
            Debug.LogWarning("[TransitionManager] ⚠ DeathSequence déjà en cours !");
            return;
        }

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        isRunning = true;

        // 1. Fondu vers noir
        yield return ui.StartBlackoutRoutine();

        // 2. Citation (spécifique ou aléatoire)
        if (quote != null)
        {
            bool done = false;
            if (quote.HasOverrideDeathQuote())
            {
                // Citation spécifique (ex : cushion onboarding) gérée depuis QuoteManager
                QuoteData specialQuote = quote.GetOverrideDeathQuote();
                quote.ClearOverrideDeathQuote();                            // Nettoyage une fois utilisée
                specialQuote.forceBackground = true;                        // S’assure que le fond noir est visible
                quote.ShowSpecificQuote(specialQuote, () => done = true);
            }
            else
            {
                // Citation aléatoire (Death)
                quote.ShowRandomQuote(QuoteType.Death, () => done = true);
            }
            yield return new WaitUntil(() => done);
        }
        else
        {
            Debug.LogWarning("[TransitionManager] ❌ QuoteManager manquant, saut de citation");
        }

        // 3. Respawn
        game.RespawnPlayer();

        // 4. Fade in
        yield return ui.StartFadeInRoutine();

        // 5. Rejoue la musique de fond
        AudioManager.Instance?.PlayMusicTheme("BackgroundTheme");

        isRunning = false;
    }

    /// <summary>
    /// Lance la séquence d’intro : blackout → citation → chargement de la scène → fade-in.
    /// </summary>

    public void PlayIntroSequence()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // 1. Fondu vers noir
        yield return ui.StartBlackoutRoutine();

        // 2. Joue la musique de fond
        AudioManager.Instance?.PlayMusicTheme("BackgroundTheme");

        // 3. Citation d’intro (si disponible)
        if (quote != null)
        {
            bool done = false;
            quote.ShowRandomQuote(QuoteType.Intro, () => done = true);
            yield return new WaitUntil(() => done);
        }
        else
        {
            Debug.LogWarning("[TransitionManager] ⚠️ QuoteManager manquant pour l’intro");
        }

        // 4. Avant le chargement de la scène, cacher le menu principal s’il est présent
        var mainMenu = FindAnyObjectByType<UIMainMenu>();
        if (mainMenu != null) mainMenu.Hide();

        // 5. Chargement de la scène
        yield return LoadSceneWithFade("Level_1");

        isRunning = false;
    }
    
    /// <summary>
    /// Charge une scène après un fondu vers noir.
    /// </summary>
    public IEnumerator LoadSceneWithFade(string sceneName)
    {
        Time.timeScale = 1f;

        yield return ui.StartBlackoutRoutine();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.1f);
        yield return ui.StartFadeInRoutine();
    }
}

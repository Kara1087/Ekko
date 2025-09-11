using System;
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

    public static Action OnSceneLoadCompleted;

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
        // Sécurité pour re-capturer les singletons s'ils n'étaient pas encore prêts à l'Awake
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
            //Debug.LogWarning("[TransitionManager] ⚠ DeathSequence déjà en cours !");
            return;
        }

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        isRunning = true;

        // 1. Fondu vers noir
        yield return ui.StartBlackoutRoutine();

        // 2. Citation (si disponible)
        if (quote)
        {
            bool done = false;
            if (game.HasOverrideDeathQuote())
            {
                // Si Cushion Onboarding, on affiche une citation spécifique
                var cushionQuote = game.GetOverrideDeathQuote();
                cushionQuote.forceBackground = true;
                quote.ShowSpecificQuote(game.GetOverrideDeathQuote(), () => done = true);
                game.ClearOverrideDeathQuote(); // pour éviter que ça reste activé
            }
            else
            {
                // Sinon, on affiche une citation aléatoire de type Death
                quote.ShowRandomQuote(QuoteType.Death, () => done = true);
            }
            yield return new WaitUntil(() => done);
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("[TransitionManager] ❌ QuoteManager manquant, saut de citation");
#endif
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

    public void PlayIntroSequence(UIMainMenu uiMainMenu)
    {
        StartCoroutine(IntroSequence(uiMainMenu));
    }

    private IEnumerator IntroSequence(UIMainMenu uiMainMenu)
    {
        // 1. Fondu vers noir
        yield return ui.StartBlackoutRoutine();

        // 2. Joue la musique de fond
        AudioManager.Instance.PlayMusicTheme("BackgroundTheme");

        // 3. Citation d’intro (si disponible)
        if (quote)
        {
            bool done = false;
            quote.ShowRandomQuote(QuoteType.Intro, () => done = true);
            yield return new WaitUntil(() => done);
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("[TransitionManager] ⚠️ QuoteManager manquant pour l’intro");
#endif
        }

        // 4. Avant le chargement de la scène, cacher le menu principal s’il est présent
        if (uiMainMenu) uiMainMenu.Hide();

        // 5. Chargement de la scène
        yield return LoadSceneWithFade("Level_1");

        isRunning = false;
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// Charge une scène après un fondu vers noir.
    /// </summary>
    public IEnumerator LoadSceneWithFade(string sceneName)
    {
        Time.timeScale = 1f;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        yield return ui.StartBlackoutRoutine();
        

        while (asyncLoad is { isDone: false })
        {
            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.1f);
        yield return ui.StartFadeInRoutine();
        
        OnSceneLoadCompleted?.Invoke();
    }
}

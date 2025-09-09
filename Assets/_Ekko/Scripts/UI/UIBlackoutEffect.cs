using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Gère les effets de fondu écran noir (fade in/out) lors des transitions de scène ou de moments-clé.
/// </summary>

public class BlackoutEffect : MonoBehaviour
{
    [SerializeField] private Image blackoutImage;
    [SerializeField] private float fadeDuration = 1.5f;

    private void Awake()
    {
        if (blackoutImage != null)
        {
            // Activation parent + image
            blackoutImage.transform.parent?.gameObject.SetActive(true);
            blackoutImage.color = new Color(0, 0, 0, 1f);  // noir opaque par défaut
            blackoutImage.gameObject.SetActive(true);     // toujours actif
        }
        else
        {
            Debug.LogWarning("[BlackoutEffect] ⚠️ blackoutImage non assigné !");
        }

    }

    private void Start()
    {
        // Auto fade-in si on arrive depuis une autre scène
        if (blackoutImage != null && blackoutImage.color.a >= 0.9f)
        {
            StartFadeIn();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// Laisse apparaître progressivement la scène en réduisant l’opacité du panneau noir.
    /// </summary>
    public void StartFadeIn(System.Action onComplete = null)
    {
        //Debug.Log("[BlackoutEffect] 🎬 Début Fade In");

        if (!IsValidTarget()) // sécurité
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("[BlackoutEffect] ⚠️ blackoutImage est null, impossible de lancer le fade-in.");
#endif
            onComplete?.Invoke();
            return;
        }

        // ✅ Active le panel parent (UI_BlackoutPanel) si désactivé
        Transform panelParent = blackoutImage.transform.parent;
        if (panelParent && !panelParent.gameObject.activeSelf)
        {
            panelParent.gameObject.SetActive(true);
        }

        blackoutImage.DOKill();

        blackoutImage.gameObject.SetActive(true);
        blackoutImage.color = new Color(0, 0, 0, 1f); // full noir

        blackoutImage.DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {   
                if (!IsValidTarget()) return;

                //Debug.Log("[BlackoutEffect] ✅ Fin Fade In");
                blackoutImage.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void StartBlackout(System.Action onComplete = null)
    {

        if (!IsValidTarget())
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("[BlackoutEffect] ⚠️ Blackout target invalid → operation cancelled.");
#endif
            onComplete?.Invoke();
            return;
        }

        blackoutImage.DOKill(); // 🔒 stoppe tout tween existant sur l’image

        blackoutImage.gameObject.SetActive(true);
        blackoutImage.color = new Color(0, 0, 0, 0f); // transparent

        blackoutImage.DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (!IsValidTarget()) return;
                    
                blackoutImage.gameObject.SetActive(false); // important sinon écran reste noir
                onComplete?.Invoke();
            });
    }

    /// <summary>
    /// Vérifie que l’image est valide et non détruite.
    /// </summary>
    private bool IsValidTarget()
    {
        return blackoutImage != null && blackoutImage.gameObject != null;
    }
    
    private void OnDestroy()
    {
        blackoutImage?.DOKill();
    }
}
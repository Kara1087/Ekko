using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BlackoutEffect : MonoBehaviour
{
    [SerializeField] private Image blackoutImage;
    [SerializeField] private float fadeDuration = 1.5f;

    private void Awake()
    {
        if (blackoutImage != null)
        {
            // Par défaut totalement opaque au démarrage
            blackoutImage.gameObject.SetActive(true);
            blackoutImage.color = new Color(0, 0, 0, 1f);
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

    private void Update()
    {
        // 🔧 Test manuel avec la touche G
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("[BlackoutEffect] Test manuel → StartBlackout()");
            StartBlackout();
        }
    }

    public void StartFadeIn(System.Action onComplete = null)
    {
        
        Debug.Log("[BlackoutEffect] 🎬 Démarrage du Fade-In");if (blackoutImage != null)
        {
            if (blackoutImage == null) return;

            blackoutImage.DOKill(); // 🔒 stoppe tout tween existant sur l’image

            blackoutImage.gameObject.SetActive(true);
            blackoutImage.color = new Color(0, 0, 0, 1f); // full noir

            blackoutImage.DOFade(0f, fadeDuration)
                .SetUpdate(true) // ignore Time.timeScale
                .OnComplete(() =>
                {
                    blackoutImage.gameObject.SetActive(false);
                    onComplete?.Invoke();
                    Debug.Log("[BlackoutEffect] ✅ Fade-in terminé.");
                });
        }
    }

    public void StartBlackout(System.Action onComplete = null)
    {
        if (blackoutImage == null) return;

        blackoutImage.DOKill(); // 🔒 stoppe tout tween existant sur l’image

        blackoutImage.gameObject.SetActive(true);
        blackoutImage.color = new Color(0, 0, 0, 0f); // transparent

        blackoutImage.DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                blackoutImage.gameObject.SetActive(false); // important sinon écran reste noir
                Debug.Log("🌀 Blackout terminé");
                onComplete?.Invoke();
            });
    }
}
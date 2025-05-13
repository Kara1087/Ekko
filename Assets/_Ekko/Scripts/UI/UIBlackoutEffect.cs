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
            // On s'assure que le parent est actif
            blackoutImage.transform.parent?.gameObject.SetActive(true);
            // Par défaut totalement opaque au démarrage
            blackoutImage.color = new Color(0, 0, 0, 1f);
            blackoutImage.gameObject.SetActive(true);     // toujours actif
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
        
        if (blackoutImage == null)
        {
            Debug.LogWarning("[BlackoutEffect] ⚠️ blackoutImage est null, impossible de lancer le fade-in.");
            onComplete?.Invoke();
            return;
        }

        // ✅ Active le panel parent (UI_BlackoutPanel) si désactivé
        Transform panelParent = blackoutImage.transform.parent;
        if (panelParent != null && !panelParent.gameObject.activeSelf)
        {
            Debug.Log("[BlackoutEffect] 🔧 Activation du UI_BlackoutPanel désactivé.");
            panelParent.gameObject.SetActive(true);
        }

        blackoutImage.DOKill();

        blackoutImage.gameObject.SetActive(true);
        blackoutImage.color = new Color(0, 0, 0, 1f); // full noir

        blackoutImage.DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                blackoutImage.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
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
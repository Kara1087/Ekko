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
            Color c = blackoutImage.color;
            c.a = 1f; // totalement opaque
            blackoutImage.color = c;
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
            blackoutImage.gameObject.SetActive(true);
            
            // 🔧 Force alpha = 1 (totalement noir)
            Color c = blackoutImage.color;
            c.a = 1f;
            blackoutImage.color = c;

            blackoutImage.DOFade(0f, fadeDuration)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            blackoutImage.gameObject.SetActive(false);
                            Debug.Log("[BlackoutEffect] 🎬 Fade-in terminé.");
                            onComplete?.Invoke();
                        });
        }
    }

    public void StartBlackout()
    {
        if (blackoutImage != null)
        {
            blackoutImage.gameObject.SetActive(true);

            blackoutImage.DOFade(1f, fadeDuration)
                         .SetUpdate(true) // permet de fonctionner même en pause (Time.timeScale = 0)
                         .OnComplete(() =>
                         {
                            blackoutImage.gameObject.SetActive(false); // <-- DÉSACTIVER LE PANEL NOIR
                            if (UIManager.Instance != null)
                                UIManager.Instance.ShowScreen(UIScreen.GameOver);
                            else
                                Debug.LogWarning("[BlackoutEffect] ❌ UIManager.Instance est null !");
                         });
        }
    }
}
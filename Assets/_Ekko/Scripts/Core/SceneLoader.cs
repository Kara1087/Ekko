using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

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

    public void LoadSceneWithFade(string sceneName, System.Action onComplete = null)
    {
        Debug.Log($"[SceneLoader] ➡️ Tentative de chargement de la scène : {sceneName}"); // 🧪
        GameManager.Instance?.StartCoroutine(LoadRoutine(sceneName, onComplete));
    }

    private IEnumerator LoadRoutine(string sceneName, System.Action onComplete)
    {
        Debug.Log($"[SceneLoader] 🔄 LoadRoutine démarrée pour {sceneName}");

        Time.timeScale = 1f;

        yield return UIManager.Instance?.StartBlackoutRoutine();
        Debug.Log("[SceneLoader] ✅ Blackout terminé");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            Debug.Log($"[SceneLoader] ⏳ Chargement en cours... {asyncLoad.progress}");
            yield return null;
        }

        Debug.Log("[SceneLoader] ✅ Scène chargée");

        yield return new WaitForSecondsRealtime(0.1f); // sécurité UI
        yield return UIManager.Instance?.StartFadeInRoutine();

        onComplete?.Invoke();
    }
}
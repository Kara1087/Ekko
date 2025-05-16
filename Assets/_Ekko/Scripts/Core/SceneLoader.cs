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
        GameManager.Instance?.StartCoroutine(LoadRoutine(sceneName, onComplete));
    }

    private IEnumerator LoadRoutine(string sceneName, System.Action onComplete)
    {
        //Debug.Log($"[SceneLoader] 🔄 LoadRoutine démarrée pour {sceneName}");

        Time.timeScale = 1f;

        yield return UIManager.Instance?.StartBlackoutRoutine();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.1f); // sécurité UI
        yield return UIManager.Instance?.StartFadeInRoutine();

        onComplete?.Invoke();
    }
}
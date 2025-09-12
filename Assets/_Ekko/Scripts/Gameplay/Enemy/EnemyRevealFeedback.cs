using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class EnemyRevealFeedback : MonoBehaviour, IAlertable
{
    [Header("Lumière d’éclairage")]
    [SerializeField] private Light2D revealLight;
    [SerializeField] private float revealDuration = 1.5f;
    [SerializeField] private float fadeSpeed = 2f;

    private Coroutine revealRoutine;

    private void Awake()
    {
        if (revealLight != null)
        {
            revealLight.enabled = false;
            Debug.Log("[EnemyRevealFeedback] 🟢 Light found and disabled at start.");
        }
        else
        {
            Debug.LogWarning("[EnemyRevealFeedback] ⚠️ No Light2D assigned!");
        }
    }

    //TODO 
    public void Alert(Vector2 sourcePosition)
    {

        if (!revealLight)
        {
            Debug.LogWarning("[EnemyRevealFeedback] ⚠️ No revealLight assigned.");
            return;
        }

        if (revealRoutine != null)
            StopCoroutine(revealRoutine);

        revealRoutine = StartCoroutine(RevealEffect());
    }

    private IEnumerator RevealEffect()
    {
        Debug.Log("[EnemyRevealFeedback] 💡 Reveal started.");
        revealLight.enabled = true;
        revealLight.intensity = 1f;

        float timer = 0f;
        while (timer < revealDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[EnemyRevealFeedback] 🔻 Starting fade out.");

        while (revealLight.intensity > 0f)
        {
            revealLight.intensity -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        revealLight.enabled = false;
        revealLight.intensity = 1f;

        Debug.Log("[EnemyRevealFeedback] 💤 Light fully faded and disabled.");
    }
}
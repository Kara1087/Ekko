using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using System.Collections;
using Unity.Cinemachine;

/// <summary>
/// Contrôle l'ouverture d'une porte en fonction de l'activation de plusieurs cristaux.
/// Chaque cristal doit implémenter IActivatableLight.
/// </summary>

public class CrystalDoorController : MonoBehaviour
{
    [Header("Cristaux requis")]
    [SerializeField] private MonoBehaviour[] crystalSources;    // Doivent implémenter IActivatableLight

    [Header("Actions")]
    [SerializeField] private UnityEvent onDoorOpened;           // Animation, son, etc.
    [SerializeField] private UnityEvent onDoorClosed;           // Si fermeture possible (optionnel)

    [Header("Paramètres")]
    [SerializeField] private bool autoCheck = true;             // Vérifie automatiquement chaque frame
    [SerializeField] private bool openOnlyOnce = true;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] doorStates; // Assigne 4 sprites dans l’ordre : 0 > 1 > 2 > 3

    [Header("Animation d’ouverture")]
    [SerializeField] private float openDelay = 1f;            // Délai avant d’ouvrir
    [SerializeField] private float moveDistance = 3f;         // Distance de descente
    [SerializeField] private float moveDuration = 1f;         // Durée du mouvement

    [Header("Caméra")]
    [SerializeField] private CameraSwitcher cameraSwitcher; // Script pour switcher temporairement la caméra

    private Vector3 initialPosition;
    private IActivatableLight[] crystals;
    private bool isOpen = false;
    public bool IsOpen() => isOpen; // Pour vérifier l'état de la porte depuis d'autres scripts

    private void Awake()
    {
        // Convertit tous les MonoBehaviours en IActivatableLight
        crystals = new IActivatableLight[crystalSources.Length];
        for (int i = 0; i < crystalSources.Length; i++)
        {
            if (crystalSources[i] is IActivatableLight light)
                crystals[i] = light;
            else
                Debug.LogWarning($"[CrystalDoorController] ❌ L'élément {crystalSources[i].name} n'implémente pas IActivatableLight");
        }

        initialPosition = transform.position;
    }

    private void Update()
    {
        if (!autoCheck || isOpen) return;

        CheckCrystals();
    }

    /// <summary>
    /// Vérifie si tous les cristaux sont activés et ouvre la porte si c’est le cas.
    /// </summary>
    public void CheckCrystals()
    {
        // 🔁 On update le sprite à chaque check, même si tous ne sont pas activés
        UpdateDoorSprite();

        foreach (var crystal in crystals)
        {
            if (crystal == null || !crystal.IsActive())
                return; // Un cristal n'est pas actif → on ne fait rien
        }

        OpenDoor();
    }

    /// <summary>
    /// Met à jour le sprite en fonction du nombre de cristaux activés.
    /// </summary>
    private void UpdateDoorSprite()
    {
        if (spriteRenderer == null || doorStates == null || doorStates.Length == 0) return;

        int activeCount = 0;
        foreach (var crystal in crystals)
        {
            if (crystal != null && crystal.IsActive())
                activeCount++;
        }

        int spriteIndex = Mathf.Clamp(activeCount, 0, doorStates.Length - 1);

        // Mise à jour du sprite
        if (spriteRenderer.sprite != doorStates[spriteIndex])
        {
            spriteRenderer.sprite = doorStates[spriteIndex];
        }
    }

    /// <summary>
    /// Active l’ouverture de la porte (événements, désactivation si unique).
    /// </summary>
    private void OpenDoor()
    {
        if (isOpen) return;

        isOpen = true;
        //Debug.Log("[CrystalDoor]🚪 Tous les cristaux sont activés → ouverture !");
        onDoorOpened?.Invoke();

        // 🔁 Focus caméra
        if (cameraSwitcher != null)
        {
            cameraSwitcher.SwitchToFocus();
        }

        StartCoroutine(AnimateDoorDescent());

        AudioManager.Instance?.StopTheme();

        if (openOnlyOnce)
            enabled = false;    // Stop le script si on ne doit plus checker



    }

    private IEnumerator AnimateDoorDescent()
    {
        yield return new WaitForSeconds(openDelay);

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * moveDistance;

        float timer = 0f;
        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / moveDuration;

            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        //Debug.Log("[CrystalDoor] 📉 Porte descendue et ouverte !");
    }

    /// <summary>
    /// Permet de réinitialiser manuellement la porte (utile pour tests ou puzzles).
    /// </summary>
    public void ForceResetDoor()
    {
        isOpen = false;
        onDoorClosed?.Invoke();
        enabled = true;

        UpdateDoorSprite(); // 🔁 met à jour le visuel à la réinitialisation
    }
    
    #if UNITY_EDITOR
    [ContextMenu("🔓 Forcer l'ouverture de la porte (debug)")]
    private void DebugForceOpenDoor()
    {
        Debug.Log("[CrystalDoorController] 🧪 Ouverture manuelle déclenchée via menu contextuel");
        OpenDoor();
    }
    #endif
}

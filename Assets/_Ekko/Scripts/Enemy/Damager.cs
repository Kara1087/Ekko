using UnityEngine;
using System.Collections;

/// <summary>
/// Composant générique infligeant des dégâts à tout objet compatible (via tag + interface IDamageable).
/// Peut être déclenché manuellement ou via collision/trigger.
/// </summary>

public class Damager : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 25f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool applyOnce = true;             // Si activé, les dégâts sont appliqués une seule fois 
    [SerializeField] private MonoBehaviour[] feedbackModulesRaw;

    private IDamagerFeedback[] feedbackModules;                 // Liste de modules visuels à activer (doivent implémenter IDamagerFeedback)
    private bool hasAppliedDamage = false;                      // Pour éviter d'appliquer plusieurs fois les dégâts


    private void Awake()
    {
        // Recherche automatique dans les enfants
        feedbackModules = GetComponentsInChildren<IDamagerFeedback>();
        //Debug.Log($"[Damager] 🔍 Feedbacks trouvés automatiquement : {feedbackModules.Length}");
    }
    
    /// <summary>
    /// Appelé automatiquement par Unity quand un collider entre dans ce trigger
    /// </summary>

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (applyOnce && hasAppliedDamage) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            ApplyDamage(playerHealth);
        }

        // On déclenche tous les feedbacks (ex : fx, son, flash...)
        foreach (var feedback in feedbackModules)
        {
            feedback?.TriggerFeedback();
        }

        if (applyOnce)
            hasAppliedDamage = true;
    }

    /// <summary>
    /// Appelé quand le joueur sort de la zone de danger
    /// Permet d’arrêter les effets visuels ou sonores (ex : feu, laser)
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        foreach (var module in feedbackModules)
        {
            if (module is IDamagerFeedback feedback)
            {
                feedback.StopFeedback();
            }
        }
    }

    /// <summary>
    /// Méthode qui applique réellement les dégâts au joueur
    /// </summary>
    public void ApplyDamage(PlayerHealth player)
    {
        player.TakeDamage(damageAmount);
    }
}

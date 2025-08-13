using UnityEngine;
using System.Collections;

/// <summary>
/// Composant qui inflige des dégâts au joueur lors d’un contact avec un trigger 2D.
/// Peut être configuré pour n’appliquer le dégât qu’une seule fois et déclencher des feedbacks visuels/sonores.
/// </summary>
/// 
public class Damager : MonoBehaviour
{
    [Header("Paramètres de dégâts")]
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

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damageAmount, transform);
        }

        // On déclenche tous les feedbacks (ex : fx, son, flash...)
        foreach (var feedback in feedbackModules)
        {
            //Debug.Log($"[Damager] → Activation de {feedback}");
            feedback?.TriggerFeedback();;
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
}

using UnityEngine;

/// <summary>
/// Gère l’apparition/disparition d’une traînée lumineuse liée à une lumière activable.
/// À attacher sur le même GameObject que celui qui implémente IActivatableLight.
/// </summary>
[RequireComponent(typeof(IActivatableLight))]
public class LightTrailController : MonoBehaviour
{
    [Header("Light Trail")]
    [SerializeField] private GameObject lightTrailPrefab;

    private IActivatableLight activatableLight;
    private GameObject lightTrailInstance;
    private FXLagFollow fxFollow;

    private void Awake()
    {
        activatableLight = GetComponent<IActivatableLight>();
    }

    private void Start()
    {
        if (activatableLight.IsActive())
            EnableTrail();
    }

    public void EnableTrail()
    {
        if (lightTrailPrefab != null && lightTrailInstance == null)
        {
            lightTrailInstance = Instantiate(lightTrailPrefab, transform.position, Quaternion.identity);

            fxFollow = lightTrailInstance.GetComponent<FXLagFollow>();
            if (fxFollow != null)
                fxFollow.target = this.transform;
        }
    }

    public void DisableTrail()
    {
        if (lightTrailInstance != null)
        {
            Destroy(lightTrailInstance);
        }
    }

    // À appeler depuis Activate/Deactivate si tu veux synchroniser manuellement
    public void OnLightStateChanged()
    {
        if (activatableLight.IsActive())
            EnableTrail();
        else
            DisableTrail();
    }
}
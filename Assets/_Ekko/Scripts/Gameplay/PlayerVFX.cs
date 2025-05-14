using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField] private GameObject lightTrailPrefab;

    private GameObject lightTrailInstance;

    void Start()
    {
        if (lightTrailPrefab != null)
        {
            lightTrailInstance = Instantiate(lightTrailPrefab, transform.position, Quaternion.identity);

            var fxFollow = lightTrailInstance.GetComponent<FXLagFollow>();
            if (fxFollow != null)
                fxFollow.target = this.transform;
        }
    }
}
using UnityEngine;

public class FXLagFollow : MonoBehaviour
{
    [SerializeField] public Transform target;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private bool snapOnStart = true;

    void Start()
    {
        if (snapOnStart && target != null)
        {
            transform.position = target.position;
        }
    }
    private void LateUpdate()
    {
        if (target == null) return;

        // Déplacement fluide (inertiel)
        transform.position = Vector3.Lerp(transform.position, target.position, followSpeed * Time.deltaTime);
    }
}
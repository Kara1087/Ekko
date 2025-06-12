using UnityEngine;

public class CrystalCameraFocus : MonoBehaviour
{
    [SerializeField] private CameraSwitcher cameraSwitcher;

    public void TriggerCameraFocus()
    {
        if (cameraSwitcher != null)
        {
            cameraSwitcher.SwitchToFocus();
        }
        else
        {
            Debug.LogWarning("[CrystalCameraFocus] ❌ Aucune référence à CameraSwitcher !");
        }
    }
}
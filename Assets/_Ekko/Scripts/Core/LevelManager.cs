using UnityEngine;

public class LevelStartManager : MonoBehaviour
{
    private void Start()
    {
        UIManager.Instance.ShowQuotePanel(false);
    }
}
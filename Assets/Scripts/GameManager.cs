using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject evictionSuccessPanel;

    private void Start()
    {
        if (evictionSuccessPanel != null)
        {
            evictionSuccessPanel.SetActive(false);
        }
    }

    public void EvictionSuccessful()
    {
        Debug.Log("EVICTION SUCCESSFUL!");

        if (evictionSuccessPanel != null)
        {
            evictionSuccessPanel.SetActive(true);
        }
    }
}

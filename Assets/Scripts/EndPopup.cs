using UnityEngine;

public class EndOfExperimentPopup : MonoBehaviour
{
    [Tooltip("Hide the popup when the scene starts.")]
    public bool hideOnStart = true;

    private void Start()
    {
        if (hideOnStart)
            gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
using UnityEngine;

public class DiceHelpUILogic : MonoBehaviour
{
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;
    [SerializeField] private GameObject helpWindow;
    [SerializeField] private GameObject matchesButton;
    [SerializeField] protected HapticManager hapticManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        OnCloseHelpPressed();
    }

    public void OnMatchesPressed()
    {
        page1.SetActive(true);
        page2.SetActive(false);
        helpWindow.SetActive(true);
        matchesButton.SetActive(false);
        hapticManager?.TriggerHaptic("light");
    }
    public void OnPageNextPressed()
    {
        page1.SetActive(false);
        page2.SetActive(true);
        helpWindow.SetActive(true);
        matchesButton.SetActive(false);
        hapticManager?.TriggerHaptic("light");
    }
    public void OnPageBackPressed()
    {
        OnMatchesPressed();
    }
    public void OnCloseHelpPressed()
    {
        page1.SetActive(false);
        page2.SetActive(false);
        helpWindow.SetActive(false);
        matchesButton.SetActive(true);
        hapticManager?.TriggerHaptic("light");
    }
}

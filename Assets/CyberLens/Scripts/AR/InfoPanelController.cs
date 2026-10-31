using TMPro;
using UnityEngine;

// Shows the name and explanation of the object the user tapped.
public class InfoPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private string hintTitle = "Explore the lab";
    [SerializeField, TextArea(2, 4)] private string hintBody = "Tap an object to learn about it. Drag to rotate. Pinch to zoom.";

    private void OnEnable()
    {
        TapTarget.Tapped += OnTargetTapped;
        ShowHint();
    }

    private void OnDisable()
    {
        TapTarget.Tapped -= OnTargetTapped;
    }

    public void ShowHint()
    {
        Show(hintTitle, hintBody);
    }

    public void Show(string title, string body)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
        if (bodyText != null)
        {
            bodyText.text = body;
        }
    }

    private void OnTargetTapped(TapTarget target)
    {
        Show(target.DisplayName, target.Explanation);
    }
}
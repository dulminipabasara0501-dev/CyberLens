using UnityEngine;
using UnityEngine.UI;

// Put this on a Button. When the button is pressed, the chosen screen opens.
[RequireComponent(typeof(Button))]
public class NavigateButton : MonoBehaviour
{
    [SerializeField] private ScreenId targetScreen = ScreenId.Welcome;

    private Button button;
    private ScreenManager screenManager;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (screenManager == null)
        {
            screenManager = FindFirstObjectByType<ScreenManager>();
        }

        if (screenManager == null)
        {
            Debug.LogError("NavigateButton: no ScreenManager found in this scene.");
            return;
        }

        screenManager.Show(targetScreen);
    }
}
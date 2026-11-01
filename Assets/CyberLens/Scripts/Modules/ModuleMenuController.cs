using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on ModuleMenuScreen.
// When a module button is pressed, it remembers the choice,
// fills in the Module Intro screen and opens it.
public class ModuleMenuController : MonoBehaviour
{
    [SerializeField] private ScreenManager screenManager;
    [SerializeField] private Button[] moduleButtons;

    [Header("Module Intro screen texts")]
    [SerializeField] private TMP_Text tagText;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text activityText;
    [SerializeField] private TMP_Text outcomeText;

    private void Awake()
    {
        for (int i = 0; i < moduleButtons.Length; i++)
        {
            int moduleIndex = i; // Copy, so each button remembers its own number.
            moduleButtons[i].onClick.AddListener(() => OnModuleClicked(moduleIndex));
        }
    }

    private void OnDestroy()
    {
        foreach (Button moduleButton in moduleButtons)
        {
            if (moduleButton != null)
            {
                moduleButton.onClick.RemoveAllListeners();
            }
        }
    }

    private void OnModuleClicked(int moduleIndex)
    {
        AppSession.SelectedModuleIndex = moduleIndex;
        LessonModule module = ModuleLibrary.GetModule(moduleIndex);

        SetText(tagText, "Module " + (moduleIndex + 1) + " of " + ModuleLibrary.Count);
        SetText(titleText, module.Title);
        SetText(activityText, module.Activity);
        SetText(outcomeText, "<b>Outcome:</b> " + module.Outcome);

        if (screenManager != null)
        {
            screenManager.Show(ScreenId.ModuleIntro);
        }
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
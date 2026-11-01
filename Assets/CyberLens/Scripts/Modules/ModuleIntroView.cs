using TMPro;
using UnityEngine;

// Put this on ModuleIntroScreen.
// Fills the intro texts for a module (or the Security Challenge) and opens the screen.
public class ModuleIntroView : MonoBehaviour
{
    [SerializeField] private ScreenManager screenManager;
    [SerializeField] private TMP_Text tagText;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text activityText;
    [SerializeField] private TMP_Text outcomeText;

    // 0 to 4 = modules. AppSession.ChallengeIndex = Security Challenge.
    public void Open(int moduleIndex)
    {
        AppSession.SelectedModuleIndex = moduleIndex;

        bool isChallenge = moduleIndex == AppSession.ChallengeIndex;
        LessonModule module = isChallenge
            ? ChallengeLibrary.GetChallenge()
            : ModuleLibrary.GetModule(moduleIndex);

        string tag = isChallenge
            ? "Final challenge"
            : "Module " + (moduleIndex + 1) + " of " + ModuleLibrary.Count;

        SetText(tagText, tag);
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
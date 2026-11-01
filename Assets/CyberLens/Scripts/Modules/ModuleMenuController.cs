using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on ModuleMenuScreen.
// Shows progress, locks and unlocks the challenge and post-quiz, and opens module intros.
public class ModuleMenuController : MonoBehaviour
{
    [SerializeField] private ModuleIntroView introView;
    [SerializeField] private Button[] moduleButtons;
    [SerializeField] private Button challengeButton;
    [SerializeField] private Button postQuizButton;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;

    [Header("Editor testing only")]
    [SerializeField] private bool editorMarkAllModulesComplete = false;

    private TMP_Text[] moduleLabels;
    private TMP_Text challengeLabel;
    private TMP_Text postQuizLabel;

    private void Awake()
    {
#if UNITY_EDITOR
        if (editorMarkAllModulesComplete)
        {
            for (int i = 0; i < ModuleLibrary.Count; i++)
            {
                ProgressStore.MarkModuleComplete(i);
            }
        }
#endif

        moduleLabels = new TMP_Text[moduleButtons.Length];
        for (int i = 0; i < moduleButtons.Length; i++)
        {
            int moduleIndex = i; // Copy, so each button remembers its own number.
            moduleLabels[i] = moduleButtons[i].GetComponentInChildren<TMP_Text>(true);
            moduleButtons[i].onClick.AddListener(() => OnModuleClicked(moduleIndex));
        }

        if (challengeButton != null)
        {
            challengeLabel = challengeButton.GetComponentInChildren<TMP_Text>(true);
            challengeButton.onClick.AddListener(OnChallengeClicked);
        }

        if (postQuizButton != null)
        {
            postQuizLabel = postQuizButton.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void OnEnable()
    {
        Refresh();
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

        if (challengeButton != null)
        {
            challengeButton.onClick.RemoveListener(OnChallengeClicked);
        }
    }

    private void Refresh()
    {
        int moduleCount = ModuleLibrary.Count;
        int completed = ProgressStore.CompletedModuleCount;

        if (progressBar != null)
        {
            progressBar.value = (float)completed / moduleCount;
        }

        string preText = ProgressStore.HasQuizResult(QuizKind.Pre)
            ? ProgressStore.GetScore(QuizKind.Pre) + "/" + QuizLibrary.Count
            : "–";
        SetText(progressText, completed + " of " + moduleCount + " complete · Pre-quiz: " + preText);

        // The first unfinished module is the recommended one.
        int recommended = -1;
        for (int i = 0; i < moduleCount; i++)
        {
            if (!ProgressStore.IsModuleComplete(i))
            {
                recommended = i;
                break;
            }
        }

        for (int i = 0; i < moduleButtons.Length; i++)
        {
            LessonModule module = ModuleLibrary.GetModule(i);
            string label = "<b>Module " + (i + 1) + ": " + module.Title + "</b>";

            if (ProgressStore.IsModuleComplete(i))
            {
                label += "\n<size=70%><color=#4FD1FF>Completed</color></size>";
            }
            else if (i == recommended)
            {
                label += "\n<size=70%><color=#4FD1FF>Recommended</color></size>";
            }

            SetText(moduleLabels[i], label);
        }

        RefreshChallenge();
        RefreshPostQuiz();
    }

    private void RefreshChallenge()
    {
        if (challengeButton == null)
        {
            return;
        }

        bool unlocked = ProgressStore.AllModulesComplete;
        string status = "Unlocks after all " + ModuleLibrary.Count + " modules";

        if (unlocked)
        {
            status = ProgressStore.IsChallengeComplete ? "Completed" : "Ready";
        }

        string color = unlocked ? "#4FD1FF" : "#B8C0EE";
        SetText(challengeLabel, "<b>Security Challenge</b>\n<size=70%><color=" + color + ">" + status + "</color></size>");
        challengeButton.interactable = unlocked;
    }

    private void RefreshPostQuiz()
    {
        if (postQuizButton == null)
        {
            return;
        }

        bool unlocked = ProgressStore.IsChallengeComplete;
        SetText(postQuizLabel, unlocked ? "Take the Post-quiz" : "Post-quiz unlocks after the challenge");
        postQuizButton.interactable = unlocked;
    }

    private void OnModuleClicked(int moduleIndex)
    {
        if (introView != null)
        {
            introView.Open(moduleIndex);
        }
    }

    private void OnChallengeClicked()
    {
        if (introView != null)
        {
            introView.Open(AppSession.ChallengeIndex);
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
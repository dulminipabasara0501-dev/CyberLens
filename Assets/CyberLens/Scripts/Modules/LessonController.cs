using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Put this on LabUI in the AR scene. It plays the chosen module step by step.
public class LessonController : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private TMP_Text moduleTitleText;
    [SerializeField] private TMP_Text stepCounterText;
    [SerializeField] private TMP_Text captionText;
    [SerializeField] private Slider progressBar;

    [Header("Lesson panel")]
    [SerializeField] private TMP_Text codeText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text tapInfoText;

    [Header("Activities")]
    [SerializeField] private ActivityController activityController;

    [Header("Question card")]
    [SerializeField] private GameObject questionCard;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button[] optionButtons;

    [Header("Navigation")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonLabel;

    [Header("Editor testing (-1 = off, 0 = Module 1 ... 4 = Module 5, 5 = Security Challenge)")]
    [SerializeField] private int editorTestModuleIndex = -1;

    private const string MainMenuSceneName = "MainMenu";

    private LessonModule module;
    private int stepIndex;
    private bool questionSolved;
    private DataTokenMover tokenMover;
    private TMP_Text[] optionLabels;
    private StyledButton[] optionStyles;
    private float normalOptionFontSize = 40f;

    private LessonStep CurrentStep
    {
        get { return module.Steps[stepIndex]; }
    }

    private bool ActivityDone
    {
        get { return activityController == null || activityController.IsComplete; }
    }

    private void Awake()
    {
        if (editorTestModuleIndex >= 0)
        {
            AppSession.SelectedModuleIndex = editorTestModuleIndex;
        }

        module = GetSelectedModule();

        CacheOptionButtons();
        AddListener(backButton, OnBackClicked);
        AddListener(nextButton, OnNextClicked);

        ShowStep(0);
    }

    private void OnEnable()
    {
        TapTarget.Tapped += OnObjectTapped;

        if (activityController != null)
        {
            activityController.StateChanged += UpdateNextButton;
        }

        // The lab only exists after it has been placed, so look for its token now.
        tokenMover = FindFirstObjectByType<DataTokenMover>();
        if (module != null)
        {
            ApplyTokenForStep(CurrentStep, true);

            if (activityController != null)
            {
                activityController.RefreshVisuals();
            }
        }
    }

    private void OnDisable()
    {
        TapTarget.Tapped -= OnObjectTapped;

        if (activityController != null)
        {
            activityController.StateChanged -= UpdateNextButton;
        }
    }

    private void OnDestroy()
    {
        RemoveListener(backButton, OnBackClicked);
        RemoveListener(nextButton, OnNextClicked);

        if (optionButtons != null)
        {
            foreach (Button optionButton in optionButtons)
            {
                if (optionButton != null)
                {
                    optionButton.onClick.RemoveAllListeners();
                }
            }
        }
    }

    private static LessonModule GetSelectedModule()
    {
        if (AppSession.SelectedModuleIndex == AppSession.ChallengeIndex)
        {
            return ChallengeLibrary.GetChallenge();
        }

        return ModuleLibrary.GetModule(AppSession.SelectedModuleIndex);
    }

    // ----- Showing a step -----

    private void ShowStep(int newIndex)
    {
        stepIndex = Mathf.Clamp(newIndex, 0, module.Steps.Length - 1);
        LessonStep step = CurrentStep;
        questionSolved = step.Question == null;

        SetText(moduleTitleText, module.Title);
        SetText(stepCounterText, "Step " + (stepIndex + 1) + " of " + module.Steps.Length);
        SetText(captionText, step.Caption);
        SetText(subtitleText, step.Subtitle);
        SetText(tapInfoText, "");
        SetText(codeText, BuildCodeText(step));

        if (progressBar != null)
        {
            progressBar.value = (float)(stepIndex + 1) / module.Steps.Length;
        }

        ShowQuestion(step.Question);
        ApplyTokenForStep(step, false);

        // The activity runs after the token is placed, so it can change the token colour.
        if (activityController != null)
        {
            activityController.ShowFor(AppSession.SelectedModuleIndex, stepIndex);
        }

        UpdateNextButton();
    }

    private string BuildCodeText(LessonStep step)
    {
        string[] lines = step.CodeLines != null ? step.CodeLines : module.CodeLines;
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < lines.Length; i++)
        {
            if (i == step.HighlightLine)
            {
                builder.Append("<mark=#00E5FF55><color=#FFFFFF>");
                builder.Append(lines[i]);
                builder.Append("</color></mark>");
            }
            else
            {
                builder.Append("<color=#B8C0EE>");
                builder.Append(lines[i]);
                builder.Append("</color>");
            }

            if (i < lines.Length - 1)
            {
                builder.Append("\n");
            }
        }

        return builder.ToString();
    }

    private void ShowQuestion(LessonQuestion question)
    {
        if (questionCard != null)
        {
            questionCard.SetActive(question != null);
        }

        if (question == null)
        {
            return;
        }

        SetText(questionText, question.Prompt);
        SetText(feedbackText, "");

        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool inUse = i < question.Options.Length;
            optionButtons[i].gameObject.SetActive(inUse);

            if (!inUse)
            {
                continue;
            }

            SetText(optionLabels[i], question.Options[i]);

            if (optionLabels[i] != null)
            {
                optionLabels[i].fontSize = question.OptionsAreCode
                    ? normalOptionFontSize * 0.7f
                    : normalOptionFontSize;
            }

            if (optionStyles[i] != null)
            {
                optionStyles[i].SetStyle(StyledButton.ButtonStyle.Secondary);
            }
        }
    }

    private void ApplyTokenForStep(LessonStep step, bool instant)
    {
        if (tokenMover == null)
        {
            return;
        }

        if (step.TokenWaypoint < 0)
        {
            tokenMover.HideToken();
        }
        else
        {
            tokenMover.MoveToWaypoint(step.TokenWaypoint, instant);
        }

        if (step.UnsafeData)
        {
            tokenMover.ShowUnsafe();
        }
        else
        {
            tokenMover.ShowSafe();
        }
    }

    // ----- Buttons -----

    private void OnOptionClicked(int optionIndex)
    {
        if (questionSolved)
        {
            return;
        }

        LessonQuestion question = CurrentStep.Question;
        if (question == null)
        {
            return;
        }

        if (optionIndex == question.CorrectIndex)
        {
            questionSolved = true;
            SetOptionStyle(optionIndex, StyledButton.ButtonStyle.Correct);
            SetText(feedbackText, "Correct. " + question.Explanation);
        }
        else
        {
            SetOptionStyle(optionIndex, StyledButton.ButtonStyle.Wrong);
            SetText(feedbackText, "Not quite. Re-read the explanation above and try again.");
        }

        UpdateNextButton();
    }

    private void OnNextClicked()
    {
        if (!questionSolved || !ActivityDone)
        {
            return;
        }

        if (stepIndex < module.Steps.Length - 1)
        {
            ShowStep(stepIndex + 1);
        }
        else
        {
            CompleteModule();
        }
    }

    private void OnBackClicked()
    {
        if (stepIndex > 0)
        {
            ShowStep(stepIndex - 1);
        }
        else
        {
            ExitToModuleMenu();
        }
    }

    private void OnObjectTapped(TapTarget target)
    {
        SetText(tapInfoText, "<b>" + target.DisplayName + ":</b> " + target.Explanation);
    }

    private void UpdateNextButton()
    {
        bool lastStep = stepIndex == module.Steps.Length - 1;
        bool canContinue = questionSolved && ActivityDone;
        string label = "Next Step";

        if (!questionSolved)
        {
            label = "Answer to continue";
        }
        else if (!ActivityDone)
        {
            label = "Complete the activity";
        }
        else if (lastStep)
        {
            label = "Finish module";
        }

        SetText(nextButtonLabel, label);

        if (nextButton != null)
        {
            nextButton.interactable = canContinue;
        }
    }

    private void CompleteModule()
    {
        // Save progress on the phone.
        if (AppSession.SelectedModuleIndex == AppSession.ChallengeIndex)
        {
            ProgressStore.MarkChallengeComplete();
        }
        else
        {
            ProgressStore.MarkModuleComplete(AppSession.SelectedModuleIndex);
        }

        ExitToModuleMenu();
    }

    private void ExitToModuleMenu()
    {
        AppSession.OpenModuleMenuOnStart = true;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    // ----- Helpers -----

    private void CacheOptionButtons()
    {
        int count = optionButtons != null ? optionButtons.Length : 0;
        optionLabels = new TMP_Text[count];
        optionStyles = new StyledButton[count];

        for (int i = 0; i < count; i++)
        {
            Button optionButton = optionButtons[i];
            optionLabels[i] = optionButton.GetComponentInChildren<TMP_Text>(true);
            optionStyles[i] = optionButton.GetComponent<StyledButton>();

            int optionIndex = i; // Copy, so each button remembers its own number.
            optionButton.onClick.AddListener(() => OnOptionClicked(optionIndex));
        }

        if (count > 0 && optionLabels[0] != null)
        {
            normalOptionFontSize = optionLabels[0].fontSize;
        }
    }

    private void SetOptionStyle(int optionIndex, StyledButton.ButtonStyle style)
    {
        if (optionIndex >= 0 && optionIndex < optionStyles.Length && optionStyles[optionIndex] != null)
        {
            optionStyles[optionIndex].SetStyle(style);
        }
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }

    private static void AddListener(Button button, UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void RemoveListener(Button button, UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }
}
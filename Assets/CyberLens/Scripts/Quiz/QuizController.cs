using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on an always-active object called QuizManager.
// Runs the pre-quiz and post-quiz, and the Replay button.
public class QuizController : MonoBehaviour
{
    [SerializeField] private ScreenManager screenManager;

    [Header("Buttons that open quizzes or replay")]
    [SerializeField] private Button[] preQuizEntryButtons;
    [SerializeField] private Button postQuizEntryButton;
    [SerializeField] private Button replayButton;

    [Header("Quiz intro screen")]
    [SerializeField] private TMP_Text introTitleText;
    [SerializeField] private TMP_Text introSubtitleText;
    [SerializeField] private Button beginQuizButton;

    [Header("Quiz screen")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonLabel;

    [Header("Options")]
    [SerializeField] private bool revealAnswersInPreQuiz = true;

    private QuizKind kind = QuizKind.Pre;
    private int questionIndex;
    private int[] answers;
    private bool answered;
    private TMP_Text[] optionLabels;
    private StyledButton[] optionStyles;

    private void Awake()
    {
        CacheOptionButtons();

        if (preQuizEntryButtons != null)
        {
            foreach (Button entryButton in preQuizEntryButtons)
            {
                AddListener(entryButton, OpenPreQuiz);
            }
        }

        AddListener(postQuizEntryButton, OpenPostQuiz);
        AddListener(replayButton, ReplayLesson);
        AddListener(beginQuizButton, BeginQuiz);
        AddListener(nextButton, OnNextClicked);
    }

    // ----- Opening -----

    public void OpenPreQuiz()
    {
        kind = QuizKind.Pre;
        ShowIntro();
    }

    public void OpenPostQuiz()
    {
        kind = QuizKind.Post;
        ShowIntro();
    }

    private void ShowIntro()
    {
        if (kind == QuizKind.Pre)
        {
            SetText(introTitleText, "Pre-quiz");
            SetText(introSubtitleText, "Checks what you already know. It is not a final grade.");
        }
        else
        {
            SetText(introTitleText, "Post-quiz");
            SetText(introSubtitleText, "Compare your result with the pre-quiz.");
        }

        screenManager.Show(ScreenId.QuizIntro);
    }

    public void BeginQuiz()
    {
        questionIndex = 0;
        answers = new int[QuizLibrary.Count];
        for (int i = 0; i < answers.Length; i++)
        {
            answers[i] = -1;
        }

        ShowQuestion();
        screenManager.Show(ScreenId.Quiz);
    }

    // Clears module progress (keeps quiz scores) and returns to the Module Menu.
    public void ReplayLesson()
    {
        ProgressStore.ResetLessonProgress();
        screenManager.Show(ScreenId.ModuleMenu);
    }

    // ----- Questions -----

    private void ShowQuestion()
    {
        QuizQuestion question = QuizLibrary.GetQuestion(questionIndex);
        answered = false;

        SetText(counterText, "Question " + (questionIndex + 1) + " of " + QuizLibrary.Count);
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
            SetOptionStyle(i, StyledButton.ButtonStyle.Secondary);
        }

        bool lastQuestion = questionIndex == QuizLibrary.Count - 1;
        SetText(nextButtonLabel, lastQuestion ? "Finish" : "Next");
        nextButton.interactable = false;
    }

    private void OnOptionClicked(int optionIndex)
    {
        if (answered)
        {
            return;
        }

        answered = true;
        answers[questionIndex] = optionIndex;

        QuizQuestion question = QuizLibrary.GetQuestion(questionIndex);
        bool correct = optionIndex == question.CorrectIndex;
        bool revealAnswer = kind == QuizKind.Post || revealAnswersInPreQuiz;

        if (revealAnswer)
        {
            SetOptionStyle(question.CorrectIndex, StyledButton.ButtonStyle.Correct);
            if (!correct)
            {
                SetOptionStyle(optionIndex, StyledButton.ButtonStyle.Wrong);
            }
        }
        else
        {
            SetOptionStyle(optionIndex, StyledButton.ButtonStyle.Primary);
        }

        if (kind == QuizKind.Pre)
        {
            SetText(feedbackText, "Answer saved.");
        }
        else if (correct)
        {
            SetText(feedbackText, "Correct.");
        }
        else
        {
            LessonModule topic = ModuleLibrary.GetModule(question.TopicModuleIndex);
            SetText(feedbackText, "Review Module " + (question.TopicModuleIndex + 1) + ": " + topic.Title + ".");
        }

        nextButton.interactable = true;
    }

    private void OnNextClicked()
    {
        if (!answered)
        {
            return;
        }

        if (questionIndex < QuizLibrary.Count - 1)
        {
            questionIndex++;
            ShowQuestion();
        }
        else
        {
            FinishQuiz();
        }
    }

    private void FinishQuiz()
    {
        int score = 0;
        List<int> missedModules = new List<int>();

        for (int i = 0; i < QuizLibrary.Count; i++)
        {
            QuizQuestion question = QuizLibrary.GetQuestion(i);

            if (answers[i] == question.CorrectIndex)
            {
                score++;
            }
            else if (!missedModules.Contains(question.TopicModuleIndex))
            {
                missedModules.Add(question.TopicModuleIndex);
            }
        }

        missedModules.Sort();
        ProgressStore.SaveQuizResult(kind, score, missedModules);

        if (kind == QuizKind.Pre)
        {
            screenManager.Show(ScreenId.ModuleMenu);
        }
        else
        {
            screenManager.Show(ScreenId.Results);
        }
    }

    // ----- Helpers -----

    private void CacheOptionButtons()
    {
        int count = optionButtons != null ? optionButtons.Length : 0;
        optionLabels = new TMP_Text[count];
        optionStyles = new StyledButton[count];

        for (int i = 0; i < count; i++)
        {
            optionLabels[i] = optionButtons[i].GetComponentInChildren<TMP_Text>(true);
            optionStyles[i] = optionButtons[i].GetComponent<StyledButton>();

            int optionIndex = i; // Copy, so each button remembers its own number.
            optionButtons[i].onClick.AddListener(() => OnOptionClicked(optionIndex));
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

    private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on ResultsScreen. Fills in the scores and makes one review button
// for each module the user got wrong in the post-quiz.
public class ResultsController : MonoBehaviour
{
    [SerializeField] private ModuleIntroView introView;
    [SerializeField] private TMP_Text preScoreText;
    [SerializeField] private TMP_Text postScoreText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text reviewTitleText;
    [SerializeField] private Button reviewButtonTemplate;

    private readonly List<GameObject> reviewButtons = new List<GameObject>();

    private void Awake()
    {
        if (reviewButtonTemplate != null)
        {
            reviewButtonTemplate.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        int preScore = ProgressStore.GetScore(QuizKind.Pre);
        int postScore = ProgressStore.GetScore(QuizKind.Post);
        int total = QuizLibrary.Count;

        SetText(preScoreText, preScore + "/" + total);
        SetText(postScoreText, postScore + "/" + total);

        if (postScore > preScore)
        {
            int gain = postScore - preScore;
            SetText(messageText, "You improved by " + gain + " point(s). Scores saved on this device.");
        }
        else
        {
            SetText(messageText, "Scores saved on this device. Review the topics below.");
        }

        ClearReviewButtons();
        List<int> missed = ProgressStore.GetMissedModules(QuizKind.Post);

        if (missed.Count == 0)
        {
            SetText(reviewTitleText, "No topics to review. Great work.");
            return;
        }

        SetText(reviewTitleText, "Topics to review");

        if (reviewButtonTemplate == null)
        {
            return;
        }

        int templateIndex = reviewButtonTemplate.transform.GetSiblingIndex();

        for (int i = 0; i < missed.Count; i++)
        {
            int moduleIndex = missed[i];
            Button clone = Instantiate(reviewButtonTemplate, reviewButtonTemplate.transform.parent);
            clone.gameObject.SetActive(true);
            clone.transform.SetSiblingIndex(templateIndex + 1 + i);

            TMP_Text label = clone.GetComponentInChildren<TMP_Text>(true);
            LessonModule module = ModuleLibrary.GetModule(moduleIndex);
            SetText(label, "Review Module " + (moduleIndex + 1) + ": " + module.Title);

            clone.onClick.AddListener(() => OnReviewClicked(moduleIndex));
            reviewButtons.Add(clone.gameObject);
        }
    }

    private void OnReviewClicked(int moduleIndex)
    {
        if (introView != null)
        {
            introView.Open(moduleIndex);
        }
    }

    private void ClearReviewButtons()
    {
        foreach (GameObject reviewButton in reviewButtons)
        {
            if (reviewButton != null)
            {
                reviewButton.SetActive(false);
                Destroy(reviewButton);
            }
        }
        reviewButtons.Clear();
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Gives a button the CyberLens look.
// Primary = glowing violet, Secondary = dark card,
// Correct = cyan highlight, Wrong = light violet highlight.
// Also plays the click sound, and the correct/wrong sound for the pressed button.
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Outline))]
public class StyledButton : MonoBehaviour, IPointerDownHandler
{
    public enum ButtonStyle
    {
        Primary,
        Secondary,
        Correct,
        Wrong
    }

    [SerializeField] private ButtonStyle style = ButtonStyle.Primary;
    [SerializeField] private TMP_Text label;

    private Button button;
    private float lastPressTime = -100f;

    private void Awake()
    {
        button = GetComponent<Button>();
        ApplyStyle();
    }

    private void OnValidate()
    {
        ApplyStyle();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            lastPressTime = Time.unscaledTime;
            AudioManager.Play(SoundId.Click);
        }
    }

    // Other scripts (for example the quiz) can call this to change the look.
    public void SetStyle(ButtonStyle newStyle)
    {
        style = newStyle;
        ApplyStyle();
        PlayFeedbackSound(newStyle);
    }

    // Only plays if this very button was just pressed by the user.
    private void PlayFeedbackSound(ButtonStyle newStyle)
    {
        if (!Application.isPlaying || Time.unscaledTime - lastPressTime > 1f)
        {
            return;
        }

        if (newStyle == ButtonStyle.Correct)
        {
            AudioManager.Play(SoundId.Correct);
        }
        else if (newStyle == ButtonStyle.Wrong)
        {
            AudioManager.Play(SoundId.Wrong);
        }
    }

    private void ApplyStyle()
    {
        Image background = GetComponent<Image>();
        Outline glow = GetComponent<Outline>();

        if (background == null || glow == null)
        {
            return;
        }

        if (label == null)
        {
            label = GetComponentInChildren<TMP_Text>();
        }

        switch (style)
        {
            case ButtonStyle.Primary:
                background.color = UIColors.Violet;
                glow.effectColor = UIColors.VioletGlow;
                glow.effectDistance = new Vector2(4f, 4f);
                break;

            case ButtonStyle.Secondary:
                background.color = UIColors.Card;
                glow.effectColor = UIColors.Line;
                glow.effectDistance = new Vector2(2f, 2f);
                break;

            case ButtonStyle.Correct:
                background.color = Color.Lerp(UIColors.Card, UIColors.Cyan, 0.1f);
                glow.effectColor = UIColors.Cyan;
                glow.effectDistance = new Vector2(2f, 2f);
                break;

            case ButtonStyle.Wrong:
                background.color = Color.Lerp(UIColors.Card, UIColors.LightViolet, 0.15f);
                glow.effectColor = UIColors.LightViolet;
                glow.effectDistance = new Vector2(2f, 2f);
                break;
        }

        if (label != null)
        {
            label.color = UIColors.TextMain;
        }
    }
}
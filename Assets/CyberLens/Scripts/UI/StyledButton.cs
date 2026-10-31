using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Gives a button the CyberLens look.
// Primary = glowing violet, Secondary = dark card,
// Correct = cyan highlight, Wrong = light violet highlight.
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Outline))]
public class StyledButton : MonoBehaviour
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

    private void Awake()
    {
        ApplyStyle();
    }

    private void OnValidate()
    {
        ApplyStyle();
    }

    // Other scripts (for example the quiz) can call this to change the look.
    public void SetStyle(ButtonStyle newStyle)
    {
        style = newStyle;
        ApplyStyle();
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
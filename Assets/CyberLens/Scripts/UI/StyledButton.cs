using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Gives a button the CyberLens look. Choose Primary or Secondary in the Inspector.
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Outline))]
public class StyledButton : MonoBehaviour
{
    public enum ButtonStyle
    {
        Primary,
        Secondary
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

        if (style == ButtonStyle.Primary)
        {
            background.color = UIColors.Violet;
            glow.effectColor = UIColors.VioletGlow;
            glow.effectDistance = new Vector2(4f, 4f);
        }
        else
        {
            background.color = UIColors.Card;
            glow.effectColor = UIColors.Line;
            glow.effectDistance = new Vector2(2f, 2f);
        }

        if (label != null)
        {
            label.color = UIColors.TextMain;
        }
    }
}
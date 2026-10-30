using UnityEngine;

// One place for every colour used in CyberLens (taken from the HTML prototype).
public static class UIColors
{
    public static readonly Color Background = FromHex("#050816");
    public static readonly Color Card = FromHex("#0E1330");
    public static readonly Color Line = FromHex("#262D5C");

    public static readonly Color Violet = FromHex("#7B3FF2");
    public static readonly Color LightViolet = FromHex("#C77DFF");
    public static readonly Color Magenta = FromHex("#E83BD6");

    public static readonly Color Cyan = FromHex("#00E5FF");
    public static readonly Color LightBlue = FromHex("#4FD1FF");

    public static readonly Color TextMain = FromHex("#F4F6FF");
    public static readonly Color TextMuted = FromHex("#B8C0EE");

    // Soft see-through versions used for glow effects.
    public static readonly Color VioletGlow = new Color(Violet.r, Violet.g, Violet.b, 0.45f);
    public static readonly Color CyanGlow = new Color(Cyan.r, Cyan.g, Cyan.b, 0.45f);

    private static Color FromHex(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        return Color.magenta; // Bright warning colour if a hex code is typed wrongly.
    }
}

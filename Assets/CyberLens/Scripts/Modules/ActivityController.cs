using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on LabUI (next to LessonController).
// Drives the ActivityPanel: tap roles, simulated login, compare, layers, arrange blocks.
public class ActivityController : MonoBehaviour
{
    [SerializeField] private GameObject activityPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button[] activityButtons; // needs 4

    // LessonController listens to this to unlock or lock the Next button.
    public event Action StateChanged;

    public bool IsComplete { get; private set; }

    private const string CyanHex = "#00E5FF";
    private const string MagentaHex = "#E83BD6";
    private const string MutedHex = "#B8C0EE";

    private static readonly string[] TapNames =
    {
        "Computer", "Application Server", "Database"
    };

    private static readonly string[] TapRoles =
    {
        "Shows the login form and collects the username and password.",
        "Checks the request and builds the database query.",
        "Stores user records and answers queries."
    };

    private static readonly string[] ArrangeBlocks =
    {
        "validate(username);",
        "query = \"SELECT * FROM users WHERE username = ?\";",
        "execute(query, [username]);",
        "return safeMessage(result);"
    };

    // The order the blocks are shown in (a shuffle of the correct order).
    private static readonly int[] ArrangeShuffle = { 2, 0, 3, 1 };

    private static readonly string[] LayerNames =
    {
        "Validation", "Least privilege", "Safe errors"
    };

    private static readonly string[] LayerMeaning =
    {
        "checks that input follows expected rules",
        "the app account has only the permissions it needs",
        "users see a generic message, not database details"
    };

    private static readonly string[] LayerRisk =
    {
        "unexpected input reaches the query layer",
        "a mistake could affect more data",
        "error messages could reveal database details"
    };

    private ActivityInfo info = new ActivityInfo();
    private bool initialised;
    private TMP_Text[] buttonLabels;
    private StyledButton[] buttonStyles;
    private float normalButtonFontSize = 34f;

    private bool[] tapped = new bool[3];
    private int lastTappedRole = -1;
    private int selectedChoice;
    private int viewedModes;
    private bool[] layerOn = new bool[3];
    private int arrangePlaced;
    private string arrangeHint = "";

    private void OnEnable()
    {
        TapTarget.Tapped += OnTargetTapped;
    }

    private void OnDisable()
    {
        TapTarget.Tapped -= OnTargetTapped;
    }

    private void OnDestroy()
    {
        if (activityButtons == null)
        {
            return;
        }

        foreach (Button activityButton in activityButtons)
        {
            if (activityButton != null)
            {
                activityButton.onClick.RemoveAllListeners();
            }
        }
    }

    // ----- Called by LessonController -----

    public void ShowFor(int moduleIndex, int stepIndex)
    {
        EnsureInitialised();
        info = ActivityLibrary.Get(moduleIndex, stepIndex);
        ResetState();

        if (info.Kind == ActivityKind.None)
        {
            if (activityPanel != null)
            {
                activityPanel.SetActive(false);
            }
            IsComplete = true;
            SetShieldGlow(true);
            return;
        }

        if (activityPanel != null)
        {
            activityPanel.SetActive(true);
        }

        switch (info.Kind)
        {
            case ActivityKind.TapRoles:
                SetText(titleText, "Tap an object in the scene, or a button");
                DrawTapRoles();
                break;
            case ActivityKind.SimulatedLogin:
                SetText(titleText, "Simulated database response");
                DrawSimulatedLogin();
                break;
            case ActivityKind.Compare:
                SetText(titleText, "Compare both versions");
                DrawCompare();
                break;
            case ActivityKind.Layers:
                DrawLayers();
                break;
            case ActivityKind.Arrange:
                DrawArrange();
                break;
        }
    }

    // Called after the lab is placed again, so token colour and shield glow come back.
    public void RefreshVisuals()
    {
        EnsureInitialised();

        switch (info.Kind)
        {
            case ActivityKind.SimulatedLogin:
                DrawSimulatedLogin();
                break;
            case ActivityKind.Compare:
                DrawCompare();
                break;
            case ActivityKind.Layers:
                DrawLayers();
                break;
            default:
                SetShieldGlow(true);
                break;
        }
    }

    // ----- Setup -----

    private void EnsureInitialised()
    {
        if (initialised)
        {
            return;
        }
        initialised = true;

        int count = activityButtons != null ? activityButtons.Length : 0;
        buttonLabels = new TMP_Text[count];
        buttonStyles = new StyledButton[count];

        for (int i = 0; i < count; i++)
        {
            buttonLabels[i] = activityButtons[i].GetComponentInChildren<TMP_Text>(true);
            buttonStyles[i] = activityButtons[i].GetComponent<StyledButton>();

            int buttonIndex = i; // Copy, so each button remembers its own number.
            activityButtons[i].onClick.AddListener(() => OnActivityButtonClicked(buttonIndex));
        }

        if (count > 0 && buttonLabels[0] != null)
        {
            normalButtonFontSize = buttonLabels[0].fontSize;
        }
    }

    private void ResetState()
    {
        for (int i = 0; i < tapped.Length; i++)
        {
            tapped[i] = false;
        }
        for (int i = 0; i < layerOn.Length; i++)
        {
            layerOn[i] = true;
        }

        lastTappedRole = -1;
        selectedChoice = info.DefaultChoice;
        viewedModes = info.Kind == ActivityKind.Compare ? (1 << info.DefaultChoice) : 0;
        arrangePlaced = 0;
        arrangeHint = "";
        IsComplete = true;
    }

    // ----- Input -----

    private void OnActivityButtonClicked(int index)
    {
        switch (info.Kind)
        {
            case ActivityKind.TapRoles:
                MarkTapped(index);
                break;
            case ActivityKind.SimulatedLogin:
                selectedChoice = index;
                DrawSimulatedLogin();
                break;
            case ActivityKind.Compare:
                selectedChoice = index;
                viewedModes |= 1 << index;
                DrawCompare();
                break;
            case ActivityKind.Layers:
                if (info.Interactive && index < layerOn.Length)
                {
                    layerOn[index] = !layerOn[index];
                    DrawLayers();
                }
                break;
            case ActivityKind.Arrange:
                PlaceBlock(index);
                break;
        }
    }

    private void OnTargetTapped(TapTarget target)
    {
        if (info.Kind != ActivityKind.TapRoles)
        {
            return;
        }

        string tappedName = target.DisplayName.Trim();
        for (int i = 0; i < TapNames.Length; i++)
        {
            if (string.Equals(tappedName, TapNames[i], StringComparison.OrdinalIgnoreCase))
            {
                MarkTapped(i);
                return;
            }
        }
    }

    // ----- Activity: tap roles -----

    private void MarkTapped(int roleIndex)
    {
        if (roleIndex < 0 || roleIndex >= tapped.Length)
        {
            return;
        }

        tapped[roleIndex] = true;
        lastTappedRole = roleIndex;
        DrawTapRoles();
    }

    private void DrawTapRoles()
    {
        int tappedCount = 0;
        for (int i = 0; i < tapped.Length; i++)
        {
            if (tapped[i])
            {
                tappedCount++;
            }
        }

        ShowButtons(3);
        for (int i = 0; i < 3; i++)
        {
            SetButton(i, TapNames[i],
                tapped[i] ? StyledButton.ButtonStyle.Correct : StyledButton.ButtonStyle.Secondary, 1f);
        }

        SetText(bodyText, lastTappedRole >= 0 ? TapRoles[lastTappedRole] : "Reveal all three roles to continue.");
        SetText(statusText, "Tapped: " + tappedCount + " of 3");
        SetComplete(tappedCount == 3);
    }

    // ----- Activity: simulated login -----

    private void DrawSimulatedLogin()
    {
        string input = selectedChoice == 0 ? SqlSimulator.NormalInput : SqlSimulator.CraftedInput;
        SimulationResult result = SqlSimulator.RunUnsafe(input);

        ShowButtons(2);
        SetButton(0, "Normal input",
            selectedChoice == 0 ? StyledButton.ButtonStyle.Primary : StyledButton.ButtonStyle.Secondary, 1f);
        SetButton(1, "Crafted input",
            selectedChoice == 1 ? StyledButton.ButtonStyle.Primary : StyledButton.ButtonStyle.Secondary, 1f);

        SetText(bodyText, BuildResultText(input, result));
        SetText(statusText, "Safe simulation. No real database is connected.");
        SetTokenUnsafe(result.IsUnsafe);
        SetShieldGlow(true);
        SetComplete(true);
    }

    // ----- Activity: compare unsafe and parameterized -----

    private void DrawCompare()
    {
        string input = SqlSimulator.CraftedInput;
        SimulationResult result = selectedChoice == 0
            ? SqlSimulator.RunUnsafe(input)
            : SqlSimulator.RunParameterized(input);

        ShowButtons(2);
        SetButton(0, "Unsafe query",
            selectedChoice == 0 ? StyledButton.ButtonStyle.Primary : StyledButton.ButtonStyle.Secondary, 1f);
        SetButton(1, "Parameterized query",
            selectedChoice == 1 ? StyledButton.ButtonStyle.Primary : StyledButton.ButtonStyle.Secondary, 1f);

        int viewedCount = ((viewedModes & 1) != 0 ? 1 : 0) + ((viewedModes & 2) != 0 ? 1 : 0);

        SetText(bodyText, BuildResultText(input, result));
        SetText(statusText, "Versions viewed: " + viewedCount + " of 2. Safe simulation only.");
        SetTokenUnsafe(result.IsUnsafe);
        SetShieldGlow(true);
        SetComplete(viewedCount == 2);
    }

    private static string BuildResultText(string input, SimulationResult result)
    {
        string color = result.IsUnsafe ? MagentaHex : CyanHex;
        string prefix = result.IsUnsafe ? "Unsafe: " : "Safe: ";

        StringBuilder builder = new StringBuilder();
        builder.Append("<b>Input:</b> ").Append(input).Append("\n");
        builder.Append("<b>Query:</b> ").Append(result.Query).Append("\n");
        builder.Append("<color=").Append(color).Append("><b>").Append(prefix).Append("</b>");
        builder.Append(result.Response).Append("</color>");
        return builder.ToString();
    }

    // ----- Activity: security layers -----

    private void DrawLayers()
    {
        SetText(titleText, info.Interactive ? "Switch the layers on and off" : "Security layers");

        StringBuilder builder = new StringBuilder();
        builder.Append("<b>Parameterized queries</b> (primary): <color=").Append(CyanHex).Append(">On</color>");
        int activeLayers = 1;

        for (int i = 0; i < LayerNames.Length; i++)
        {
            bool added = info.Interactive || (info.LayerMask & (1 << i)) != 0;
            bool on = info.Interactive ? layerOn[i] : added;

            if (on)
            {
                activeLayers++;
            }

            builder.Append("\n<b>").Append(LayerNames[i]).Append("</b>: ");

            if (!added)
            {
                builder.Append("<color=").Append(MutedHex).Append(">Not added yet</color>");
            }
            else if (on)
            {
                builder.Append("<color=").Append(CyanHex).Append(">On</color> - ").Append(LayerMeaning[i]);
            }
            else
            {
                builder.Append("<color=").Append(MagentaHex).Append(">Off</color> - ").Append(LayerRisk[i]);
            }
        }

        SetText(bodyText, builder.ToString());
        SetText(statusText, "Protection: " + activeLayers + " of 4 layers active");

        if (info.Interactive)
        {
            ShowButtons(3);
            for (int i = 0; i < 3; i++)
            {
                SetButton(i, LayerNames[i] + ": " + (layerOn[i] ? "On" : "Off"),
                    layerOn[i] ? StyledButton.ButtonStyle.Correct : StyledButton.ButtonStyle.Secondary, 1f);
            }
        }
        else
        {
            ShowButtons(0);
        }

        SetShieldGlow(activeLayers == 4);
        SetComplete(true);
    }

    // ----- Activity: arrange the blocks -----

    private void DrawArrange()
    {
        SetText(titleText, "Tap the blocks in the correct order");

        StringBuilder builder = new StringBuilder();
        builder.Append("<b>Workflow so far:</b>");

        if (arrangePlaced == 0)
        {
            builder.Append(" (nothing yet)");
        }

        for (int i = 0; i < arrangePlaced; i++)
        {
            builder.Append("\n<color=").Append(CyanHex).Append(">").Append(i + 1).Append(". ");
            builder.Append(ArrangeBlocks[i]).Append("</color>");
        }

        SetText(bodyText, builder.ToString());

        ShowButtons(4);
        for (int i = 0; i < 4; i++)
        {
            int block = ArrangeShuffle[i];
            bool alreadyPlaced = block < arrangePlaced;

            if (alreadyPlaced)
            {
                activityButtons[i].gameObject.SetActive(false);
            }
            else
            {
                SetButton(i, ArrangeBlocks[block], StyledButton.ButtonStyle.Secondary, 0.75f);
            }
        }

        bool finished = arrangePlaced == ArrangeBlocks.Length;
        SetText(statusText, finished
            ? "Correct: validate, build with a placeholder, execute with a parameter, return a safe result."
            : arrangeHint);
        SetComplete(finished);
    }

    private void PlaceBlock(int buttonIndex)
    {
        if (arrangePlaced >= ArrangeBlocks.Length || buttonIndex < 0 || buttonIndex >= ArrangeShuffle.Length)
        {
            return;
        }

        if (ArrangeShuffle[buttonIndex] == arrangePlaced)
        {
            arrangePlaced++;
            arrangeHint = "";
        }
        else
        {
            arrangeHint = "Not quite: validate first, build the query with a placeholder, execute with a parameter, then return a safe result.";
        }

        DrawArrange();
    }

    // ----- 3D lab helpers -----

    private void SetTokenUnsafe(bool isUnsafe)
    {
        DataTokenMover token = FindFirstObjectByType<DataTokenMover>();
        if (token == null)
        {
            return;
        }

        if (isUnsafe)
        {
            token.ShowUnsafe();
        }
        else
        {
            token.ShowSafe();
        }
    }

    private void SetShieldGlow(bool glowing)
    {
        DataTokenMover token = FindFirstObjectByType<DataTokenMover>();
        if (token == null || token.transform.parent == null)
        {
            return;
        }

        Transform inner = FindDeep(token.transform.parent, "Shield_Inner");
        if (inner != null)
        {
            inner.gameObject.SetActive(glowing);
        }
    }

    private static Transform FindDeep(Transform root, string childName)
    {
        foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
        {
            if (candidate.name == childName)
            {
                return candidate;
            }
        }
        return null;
    }

    // ----- UI helpers -----

    private void SetComplete(bool complete)
    {
        bool changed = complete != IsComplete;
        IsComplete = complete;

        if (changed && StateChanged != null)
        {
            StateChanged();
        }
    }

    private void ShowButtons(int count)
    {
        for (int i = 0; i < activityButtons.Length; i++)
        {
            activityButtons[i].gameObject.SetActive(i < count);
        }
    }

    private void SetButton(int index, string label, StyledButton.ButtonStyle style, float fontScale)
    {
        if (index < 0 || index >= activityButtons.Length)
        {
            return;
        }

        SetText(buttonLabels[index], label);

        if (buttonLabels[index] != null)
        {
            buttonLabels[index].fontSize = normalButtonFontSize * fontScale;
        }
        if (buttonStyles[index] != null)
        {
            buttonStyles[index].SetStyle(style);
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
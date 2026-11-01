public enum ActivityKind
{
    None,
    TapRoles,
    SimulatedLogin,
    Compare,
    Layers,
    Arrange
}

// Describes the activity on one lesson step.
public class ActivityInfo
{
    public ActivityKind Kind = ActivityKind.None;
    public int DefaultChoice;   // SimulatedLogin: 0 = normal input, 1 = crafted input
    public int LayerMask;       // Layers: 1 = validation, 2 = least privilege, 4 = safe errors
    public bool Interactive;    // Layers: true = the user can switch layers on and off
}

// Says which module step has which activity.
// moduleIndex: 0 to 4 = Modules 1 to 5. stepIndex: 0 = Step 1, 1 = Step 2 ...
public static class ActivityLibrary
{
    public static ActivityInfo Get(int moduleIndex, int stepIndex)
    {
        switch (moduleIndex)
        {
            case 0: // Module 1: tap each object
                if (stepIndex == 3)
                {
                    return Make(ActivityKind.TapRoles, 0, 0, false);
                }
                break;

            case 1: // Module 2: simulated login
                if (stepIndex == 0)
                {
                    return Make(ActivityKind.SimulatedLogin, 0, 0, false);
                }
                if (stepIndex == 2)
                {
                    return Make(ActivityKind.SimulatedLogin, 1, 0, false);
                }
                break;

            case 2: // Module 3: unsafe vs safe comparison
                if (stepIndex == 3)
                {
                    return Make(ActivityKind.Compare, 0, 0, false);
                }
                break;

            case 3: // Module 4: security layers
                if (stepIndex == 1)
                {
                    return Make(ActivityKind.Layers, 0, 1, false);
                }
                if (stepIndex == 2)
                {
                    return Make(ActivityKind.Layers, 0, 3, false);
                }
                if (stepIndex == 3)
                {
                    return Make(ActivityKind.Layers, 0, 7, false);
                }
                if (stepIndex == 4)
                {
                    return Make(ActivityKind.Layers, 0, 7, true);
                }
                break;

            case 4: // Module 5: arrange the blocks
                if (stepIndex == 2)
                {
                    return Make(ActivityKind.Arrange, 0, 0, false);
                }
                break;
        }

        return new ActivityInfo();
    }

    private static ActivityInfo Make(ActivityKind kind, int defaultChoice, int layerMask, bool interactive)
    {
        ActivityInfo info = new ActivityInfo();
        info.Kind = kind;
        info.DefaultChoice = defaultChoice;
        info.LayerMask = layerMask;
        info.Interactive = interactive;
        return info;
    }
}
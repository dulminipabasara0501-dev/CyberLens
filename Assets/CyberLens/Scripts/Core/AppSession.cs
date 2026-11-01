// Remembers choices while the app moves between scenes (while it is running).
public static class AppSession
{
    // SelectedModuleIndex 0 to 4 = Modules 1 to 5. This number = the Security Challenge.
    public const int ChallengeIndex = 5;

    public static int SelectedModuleIndex = 0;

    // When true, the MainMenu scene opens on the Module Menu instead of Welcome.
    public static bool OpenModuleMenuOnStart = false;
}
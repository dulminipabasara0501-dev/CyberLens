// Remembers choices while the app moves between scenes (while it is running).
public static class AppSession
{
    // 0 = Module 1, 1 = Module 2, and so on.
    public static int SelectedModuleIndex = 0;

    // When true, the MainMenu scene opens on the Module Menu instead of Welcome.
    public static bool OpenModuleMenuOnStart = false;
}
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Handles the Android Back button (Unity reports it as the Escape key).
// MainMenu scene: assign the ScreenManager. AR scene: leave ScreenManager empty.
public class BackKeyHandler : MonoBehaviour
{
    [Header("MainMenu scene: drag the ScreenManager here")]
    [SerializeField] private ScreenManager screenManager;

    [Header("AR scene: the scene to open (ScreenManager stays empty)")]
    [SerializeField] private string sceneToLoad = "MainMenu";

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        if (screenManager != null)
        {
            HandleScreenBack();
        }
        else
        {
            LeaveScene();
        }
    }

    private void HandleScreenBack()
    {
        switch (screenManager.CurrentScreen)
        {
            case ScreenId.Welcome:
                Application.Quit();
                break;

            case ScreenId.HowItWorks:
                screenManager.Show(ScreenId.Welcome);
                break;

            case ScreenId.ChooseMode:
                screenManager.Show(ScreenId.HowItWorks);
                break;

            case ScreenId.CameraAccess:
                screenManager.Show(ScreenId.ChooseMode);
                break;

            case ScreenId.QuizIntro:
            case ScreenId.ModuleIntro:
            case ScreenId.Help:
                screenManager.Show(ScreenId.PreviousScreen);
                break;

            case ScreenId.Quiz:
                // Do nothing, so answers are not lost by accident.
                break;

            case ScreenId.ModuleMenu:
            case ScreenId.Finish:
                screenManager.Show(ScreenId.Welcome);
                break;

            case ScreenId.Results:
                screenManager.Show(ScreenId.ModuleMenu);
                break;
        }
    }

    private void LeaveScene()
    {
        AppSession.OpenModuleMenuOnStart = true;
        SceneManager.LoadScene(sceneToLoad);
    }
}
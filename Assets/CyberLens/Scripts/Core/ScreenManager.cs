using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The names of all screens in the MainMenu scene.
// Each name + "Screen" must match a GameObject under "Screens"
// (for example HowItWorks -> HowItWorksScreen).
public enum ScreenId
{
    Welcome,
    HowItWorks,
    ChooseMode,
    CameraAccess,
    QuizIntro,
    Quiz,
    ModuleMenu,
    ModuleIntro,
    Results,
    Finish,
    Help,
    PreviousScreen // Special value: go back to the screen we came from.
}

// Shows one screen at a time and fades it in.
public class ScreenManager : MonoBehaviour
{
    [SerializeField] private Transform screensRoot;
    [SerializeField] private ScreenId startScreen = ScreenId.Welcome;
    [SerializeField] private float fadeSeconds = 0.25f;

    private readonly Dictionary<ScreenId, GameObject> screens = new Dictionary<ScreenId, GameObject>();
    private ScreenId currentScreen = ScreenId.Welcome;
    private ScreenId previousScreen = ScreenId.Welcome;
    private bool hasShownFirstScreen;
    private Coroutine fadeRoutine;

    public ScreenId CurrentScreen
    {
        get { return currentScreen; }
    }

    private void Awake()
    {
        FindAllScreens();
    }

    private void Start()
    {
        ScreenId firstScreen = startScreen;
        if (firstScreen == ScreenId.PreviousScreen)
        {
            firstScreen = ScreenId.Welcome;
        }

        // Coming back from the AR lesson: open the Module Menu directly.
        if (AppSession.OpenModuleMenuOnStart)
        {
            AppSession.OpenModuleMenuOnStart = false;
            firstScreen = ScreenId.ModuleMenu;
        }

        Show(firstScreen);
    }

    // Shows the chosen screen and hides all the others.
    public void Show(ScreenId target)
    {
        if (target == ScreenId.PreviousScreen)
        {
            target = previousScreen;
        }

        if (!screens.ContainsKey(target))
        {
            Debug.LogError("ScreenManager: screen not found: " + target);
            return;
        }

        if (hasShownFirstScreen && target != currentScreen)
        {
            previousScreen = currentScreen;
        }

        foreach (KeyValuePair<ScreenId, GameObject> entry in screens)
        {
            entry.Value.SetActive(entry.Key == target);
        }

        currentScreen = target;
        hasShownFirstScreen = true;
        StartFade(screens[target]);
    }

    private void FindAllScreens()
    {
        screens.Clear();

        if (screensRoot == null)
        {
            Debug.LogError("ScreenManager: drag the 'Screens' object into the Screens Root field.");
            return;
        }

        foreach (ScreenId id in System.Enum.GetValues(typeof(ScreenId)))
        {
            if (id == ScreenId.PreviousScreen)
            {
                continue;
            }

            string objectName = id.ToString() + "Screen";
            Transform found = screensRoot.Find(objectName);

            if (found == null)
            {
                Debug.LogError("ScreenManager: cannot find '" + objectName + "' under Screens. Check the spelling.");
                continue;
            }

            screens[id] = found.gameObject;
        }
    }

    private void StartFade(GameObject screenObject)
    {
        if (fadeSeconds <= 0f)
        {
            return;
        }

        CanvasGroup group = screenObject.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = screenObject.AddComponent<CanvasGroup>();
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(FadeIn(group));
    }

    private IEnumerator FadeIn(CanvasGroup group)
    {
        float elapsed = 0f;
        group.alpha = 0f;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(elapsed / fadeSeconds);
            yield return null;
        }

        group.alpha = 1f;
    }
}
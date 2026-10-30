using UnityEngine;
using UnityEngine.SceneManagement;

// Shows the splash scene for a short time, then loads the main menu scene.
public class SplashController : MonoBehaviour
{
    [SerializeField] private float splashDurationSeconds = 2f;
    [SerializeField] private string nextSceneName = "MainMenu";

    private void Start()
    {
        Invoke(nameof(LoadNextScene), splashDurationSeconds);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}

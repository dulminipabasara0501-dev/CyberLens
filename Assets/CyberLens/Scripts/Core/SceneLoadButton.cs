using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Put this on a Button. When pressed, it loads the scene with the given name.
// The scene must be listed in File > Build Profiles.
[RequireComponent(typeof(Button))]
public class SceneLoadButton : MonoBehaviour
{
    [SerializeField] private string sceneName = "MainMenu";

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        SceneManager.LoadScene(sceneName);
    }
}
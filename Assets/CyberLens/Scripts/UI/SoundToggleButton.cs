using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on a Button. Pressing it switches all sound on or off.
[RequireComponent(typeof(Button))]
public class SoundToggleButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (label == null)
        {
            label = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnButtonClicked);
        RefreshLabel();
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        AudioManager.SetSoundOn(!AudioManager.IsSoundOn);
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (label != null)
        {
            label.text = AudioManager.IsSoundOn ? "Sound: On" : "Sound: Off";
        }
    }
}
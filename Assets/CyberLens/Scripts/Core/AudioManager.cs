using UnityEngine;
using UnityEngine.SceneManagement;

public enum SoundId
{
    Click,
    Tap,
    Correct,
    Wrong,
    Success
}

// Creates itself when the app starts and stays alive between scenes.
// Plays sound effects and quiet background music. The sound files live in
// Assets/CyberLens/Resources/Audio.
public class AudioManager : MonoBehaviour
{
    private const string SoundOnKey = "cl_sound_on";
    private const float EffectsVolume = 0.8f;
    private const float MusicVolume = 0.25f;

    private static AudioManager instance;

    private AudioSource effectsSource;
    private AudioSource musicSource;
    private AudioListener ownListener;
    private AudioClip[] effectClips;
    private AudioClip musicClip;
    private bool soundOn = true;

    public static bool IsSoundOn
    {
        get
        {
            if (instance != null)
            {
                return instance.soundOn;
            }
            return PlayerPrefs.GetInt(SoundOnKey, 1) == 1;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateAtStartup()
    {
        if (instance != null)
        {
            return;
        }

        GameObject holder = new GameObject("AudioManager");
        holder.AddComponent<AudioManager>();
    }

    public static void Play(SoundId sound)
    {
        if (instance != null)
        {
            instance.PlayEffect(sound);
        }
    }

    public static void SetSoundOn(bool on)
    {
        if (instance != null)
        {
            instance.ApplySoundSetting(on);
        }
        else
        {
            PlayerPrefs.SetInt(SoundOnKey, on ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        soundOn = PlayerPrefs.GetInt(SoundOnKey, 1) == 1;

        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = MusicVolume;

        // Used only when a scene has no audio listener of its own.
        ownListener = gameObject.AddComponent<AudioListener>();
        ownListener.enabled = false;

        effectClips = new AudioClip[5];
        effectClips[(int)SoundId.Click] = LoadClip("click");
        effectClips[(int)SoundId.Tap] = LoadClip("tap");
        effectClips[(int)SoundId.Correct] = LoadClip("correct");
        effectClips[(int)SoundId.Wrong] = LoadClip("wrong");
        effectClips[(int)SoundId.Success] = LoadClip("success");
        musicClip = LoadClip("music");

        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }

        ApplyMute();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateListener();
    }

    // Keeps exactly one audio listener in the scene.
    private void UpdateListener()
    {
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        int otherListeners = 0;

        foreach (AudioListener listener in listeners)
        {
            if (listener != ownListener && listener.enabled)
            {
                otherListeners++;
            }
        }

        ownListener.enabled = otherListeners == 0;
    }

    private void PlayEffect(SoundId sound)
    {
        if (!soundOn)
        {
            return;
        }

        AudioClip clip = effectClips[(int)sound];
        if (clip != null)
        {
            effectsSource.PlayOneShot(clip, EffectsVolume);
        }
    }

    private void ApplySoundSetting(bool on)
    {
        soundOn = on;
        PlayerPrefs.SetInt(SoundOnKey, on ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMute();

        if (on && musicClip != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    private void ApplyMute()
    {
        effectsSource.mute = !soundOn;
        musicSource.mute = !soundOn;
    }

    private static AudioClip LoadClip(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + clipName);
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: missing sound file Resources/Audio/" + clipName);
        }
        return clip;
    }
}
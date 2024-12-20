using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private static AudioSource backgroundMusic;
    private static AudioSource carCrashSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            backgroundMusic = GetAudioSourceByClipName("Starting Line");
            carCrashSound = GetAudioSourceByClipName("CarCrash");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public AudioSource GetAudioSourceByClipName(string clipName)
    {
        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source.clip != null && source.clip.name == clipName)
            {
                return source;
            }
        }
        Debug.LogError($"AudioSource with clip name '{clipName}' not found!");
        return null;
    }

    public void PlaySpecificClip(string clipName)
    {
        AudioSource source = AudioManager.Instance.GetAudioSourceByClipName(clipName);
        if (source != null)
        {
            source.Play();
            source.volume = 1f;
        }
    }

    public void StopSpecificClip(string clipName)
    {
        AudioSource source = AudioManager.Instance.GetAudioSourceByClipName(clipName);
        if (source != null)
        {
            source.Stop();
            source.volume = 0f;
        }
    }
}
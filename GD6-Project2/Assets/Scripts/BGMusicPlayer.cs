using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMusicPlayer : MonoBehaviour
{
    private static BGMusicPlayer instance;
    private AudioSource src;

    [Tooltip("Background music clip to play")] public AudioClip musicClip;
    [Range(0f, 1f)] public float volume = 1f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            src = GetComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.volume = volume;
            if (musicClip != null)
            {
                src.clip = musicClip;
                src.Play();
            }
            else
            {
                Debug.LogWarning("BGMusicPlayer: No music clip assigned.", this);
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnValidate()
    {
        if (TryGetComponent(out AudioSource a))
        {
            a.volume = volume;
            a.loop = true;
            a.playOnAwake = false;
        }
    }

    public static void PlayClip(AudioClip clip)
    {
        if (instance != null && instance.src != null && clip != null)
        {
            instance.src.clip = clip;
            instance.src.Play();
        }
    }

    public static void StopMusic()
    {
        if (instance != null && instance.src != null) instance.src.Stop();
    }
}

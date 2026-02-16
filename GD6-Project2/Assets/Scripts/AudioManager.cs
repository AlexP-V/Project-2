using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource footstepSource;
    public AudioSource fakeSource;
    public AudioSource startSource;
    public AudioSource winSource;
    public AudioSource clickSource;

    public AudioClip footstepClip;
    public AudioClip fakeClip;
    public AudioClip startClip;
    public AudioClip winClip;
    public AudioClip clickClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayFootstepSound()
    {
        footstepSource.PlayOneShot(footstepClip);
    }

    public void PlayFakeSound()
    {
        fakeSource.PlayOneShot(fakeClip);
    }

    public void PlayStartSound()
    {
        startSource.PlayOneShot(startClip);
    }

    public void PlayWinSound()
    {
        winSource.PlayOneShot(winClip);
    }

    public void PlayClickSound()
    {
        clickSource.PlayOneShot(clickClip);
    }
}

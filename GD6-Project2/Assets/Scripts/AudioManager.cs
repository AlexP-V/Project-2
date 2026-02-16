using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource footstepSource;
    public AudioSource fakeSource;

    public AudioClip footstepClip;
    public AudioClip fakeClip;
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
}

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip dashSound;

    private void Awake()
    {
        // Singleton pattern to make it easy to call from any script
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        // Play music on start if assigned
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }
}

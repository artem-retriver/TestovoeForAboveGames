using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip tabClickSound;
    [SerializeField] private AudioClip bannerSwipeSound;
    [SerializeField] private AudioClip imageClickSound;
    [SerializeField] private AudioClip imageCloseSound;
    [SerializeField] private AudioClip imageNotReadySound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource == null || backgroundMusic == null) 
            return;
        
        if (!musicSource.isPlaying || musicSource.clip != backgroundMusic)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void PlayTabClick()
    {
        PlaySfx(tabClickSound);
    }

    public void PlayBannerSwipe()
    {
        PlaySfx(bannerSwipeSound);
    }

    public void PlayImageClick()
    {
        PlaySfx(imageClickSound);
    }

    public void PlayImageNotReady()
    {
        PlaySfx(imageNotReadySound);
    }

    public void PlayImageClose()
    {
        PlaySfx(imageCloseSound);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) 
            return;
        
        sfxSource.PlayOneShot(clip);
    }
}

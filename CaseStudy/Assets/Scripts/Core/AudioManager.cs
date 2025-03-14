using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Variables // Değişkenler: Müzik ve ses efektleri AudioClip'leri
    public AudioClip backgroundMusic;
    public AudioClip blockBlastSound;
    public AudioClip blockDüşmeSound;

    private AudioSource musicSource;
    #endregion

    #region Singleton // Singleton Pattern Bölgesi - AudioManager Instance (Tek Örnek) Yönetimi

    public static AudioManager Instance { get; private set; } 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this; 
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true; 
            musicSource.clip = backgroundMusic;
        }
    }

    #endregion


    public void PlayBackgroundMusic() 
    {
        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        musicSource.Stop();
    }

    public void PlayBlockBlastSound() 
    {
        PlaySoundEffect(blockBlastSound);
    }

    public void PlayBlockDüşmeSound()
    {
        PlaySoundEffect(blockDüşmeSound);
    }

    private void PlaySoundEffect(AudioClip clip) 
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Vector3.zero); 
        }
    }
}

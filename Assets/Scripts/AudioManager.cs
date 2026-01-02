using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volumes")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    //musica al empezar
    [SerializeField] private AudioClip MusicaFondo;

    

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (!musicSource || !sfxSource)
        {
            Debug.LogWarning("AudioManager: asigná Music Source y Sfx Source en el Inspector.", this);
        }

        ApplyVolumes();
    }


    void Start()
    {
        if (MusicaFondo != null)
            PlayMusic(MusicaFondo, loop: true);

    }


    public void PlaySfx(AudioClip clip, float volume01 = 1f, float pitchMin = 0.9f, float pitchMax = 1.1f)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume01) * sfxVolume);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void SetSfxVolume(float v) { sfxVolume = Mathf.Clamp01(v); ApplyVolumes(); }
    public void SetMusicVolume(float v) { musicVolume = Mathf.Clamp01(v); ApplyVolumes(); }

    private void ApplyVolumes()
    {
        if (musicSource) musicSource.volume = musicVolume;
        // sfxSource volumen base se aplica al PlayOneShot via sfxVolume
    }
}

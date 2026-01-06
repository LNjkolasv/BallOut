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

    [Header("Music")]
    [SerializeField] private AudioClip musicaFondo;

    private void Awake()
    {
        // 🔒 Singleton seguro
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        if (!musicSource || !sfxSource)
        {
            Debug.LogWarning("AudioManager: asigná Music Source y Sfx Source en el Inspector.", this);
        }

        ApplyVolumes();
    }

    private void Start()
    {
        // ▶️ Solo reproducir si no hay música sonando
        if (musicaFondo != null && musicSource != null && !musicSource.isPlaying)
        {
            PlayMusic(musicaFondo, loop: true);
        }
    }

    // 🔊 SFX
    public void PlaySfx(AudioClip clip, float volume01 = 1f, float pitchMin = 0.9f, float pitchMax = 1.1f)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.pitch = Random.Range(pitchMin, pitchMax);
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume01) * sfxVolume);
    }

    // 🎵 Música
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;

        // Evita reiniciar la misma música
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    // 🎚️ Volúmenes
    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        if (musicSource) musicSource.volume = musicVolume;
        // El volumen de SFX se aplica al PlayOneShot
    }
}

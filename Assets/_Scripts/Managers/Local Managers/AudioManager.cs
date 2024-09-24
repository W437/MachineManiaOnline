using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class MenuSFXClip
{
    public AudioManager.MenuSFX sfxType;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    [Header("Audio Mixer")]
    [SerializeField] AudioMixer mainMixer;

    [Header("Mixer Groups")]
    [SerializeField] AudioMixerGroup inGameMusicGroup;
    [SerializeField] AudioMixerGroup inGameSFXGroup;
    [SerializeField] AudioMixerGroup menuSFXGroup;
    [SerializeField] AudioMixerGroup masterGroup;

    [Header("Menu SFX Clips")]
    [SerializeField] List<MenuSFXClip> menuSFXClips;
    Dictionary<MenuSFX, AudioClip> menuSFXClipMap;

    [Header("Music Tracks")]
    [SerializeField] List<AudioClip> musicTracks;
    [SerializeField] public AudioClip yaketySax;
    int _lastTrackIndex = -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMenuSFXClipMap();
            AssignAudioMixerGroups();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayRandomMusic();
    }

    public void SetCutoffFrequency(float toValue, float duration = 1f)
    {
        mainMixer.GetFloat("lowpass", out float value);
        LeanTween.value(gameObject, value, toValue, duration).setOnUpdate((float value) =>
        {
            mainMixer.SetFloat("lowpass", value);
        });
    }

    public void SetVolume(string parameterName, float volume)
    {
        mainMixer.SetFloat(parameterName, volume);
    }

    public void PlayMenuSFX(MenuSFX sfx)
    {
        if (sfxSource.isPlaying) return; // Prevent overlap

        if (menuSFXClipMap.TryGetValue(sfx, out AudioClip clip))
        {
            PlaySFX(clip, menuSFXGroup);
        }
    }

    public void StopGameSFX()
    {
        if (sfxSource.isPlaying)
        {
            sfxSource.Stop();
        }
    }
    public void PlayGameSFX(AudioClip gameSFX)
    {
        if (sfxSource.isPlaying)
        {
            sfxSource.Stop();
        }

        PlaySFX(gameSFX, inGameSFXGroup);
    }

    public void PlaySpecificSoundtrack(AudioClip track)
    {
        StartCoroutine(FadeInMusic(track));
    }

    void InitializeMenuSFXClipMap()
    {
        menuSFXClipMap = new Dictionary<MenuSFX, AudioClip>();
        foreach (var menuSFXClip in menuSFXClips)
        {
            if (!menuSFXClipMap.ContainsKey(menuSFXClip.sfxType))
            {
                menuSFXClipMap.Add(menuSFXClip.sfxType, menuSFXClip.clip);
            }
        }
    }

    void AssignAudioMixerGroups()
    {
        musicSource.outputAudioMixerGroup = inGameMusicGroup;
        sfxSource.outputAudioMixerGroup = inGameSFXGroup;
    }

    void PlaySFX(AudioClip clip, AudioMixerGroup group = null)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.outputAudioMixerGroup = group ?? inGameSFXGroup;
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayRandomMusic()
    {
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, musicTracks.Count);
        } while (randomIndex == _lastTrackIndex);

        _lastTrackIndex = randomIndex;
        AudioClip track = musicTracks[randomIndex];

        StartCoroutine(FadeInMusic(track));
    }

    IEnumerator FadeInMusic(AudioClip newTrack)
    {
        if (musicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic());
        }

        musicSource.clip = newTrack;
        musicSource.Play();
        musicSource.volume = 0f;

        float fadeDuration = 0.5f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            musicSource.volume = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        musicSource.volume = 1f;
        StartCoroutine(PlayNextTrackWithFade());
    }

    IEnumerator FadeOutMusic()
    {
        float fadeDuration = 2f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            musicSource.volume = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();
    }

    IEnumerator PlayNextTrackWithFade()
    {
        while (musicSource.isPlaying)
        {
            if (musicSource.time >= musicSource.clip.length - 2f)
            {
                yield return StartCoroutine(FadeOutMusic());
                PlayRandomMusic();
                yield break;
            }
            yield return null;
        }
    }

    public enum MenuSFX
    {
        Click,
        Info,
        Warning,
        Success,
        Error,
        InGameSFXOne
    }
}

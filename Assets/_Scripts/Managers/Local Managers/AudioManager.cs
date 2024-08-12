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
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup inGameMusicGroup;
    [SerializeField] private AudioMixerGroup inGameSFXGroup;
    [SerializeField] private AudioMixerGroup menuSFXGroup;
    [SerializeField] private AudioMixerGroup masterGroup;

    [Header("Menu SFX Clips")]
    [SerializeField] private List<MenuSFXClip> menuSFXClips;

    [Header("Music Tracks")]
    [SerializeField] private List<AudioClip> musicTracks;

    private int _lastTrackIndex = -1;

    public enum MenuSFX
    {
        Click,
        Error
    }

    private Dictionary<MenuSFX, AudioClip> menuSFXClipMap;

    private void Awake()
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

    private void Start()
    {
        PlayRandomMusic();
    }

    private void InitializeMenuSFXClipMap()
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

    private void AssignAudioMixerGroups()
    {
        musicSource.outputAudioMixerGroup = inGameMusicGroup;
        sfxSource.outputAudioMixerGroup = inGameSFXGroup;
    }

    public void PlayMenuSFX(MenuSFX sfx)
    {
        if (menuSFXClipMap.TryGetValue(sfx, out AudioClip clip))
        {
            PlaySFX(clip, menuSFXGroup);
        }
    }

    private void PlaySFX(AudioClip clip, AudioMixerGroup group = null)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.outputAudioMixerGroup = group ?? inGameSFXGroup;
            sfxSource.PlayOneShot(clip);
        }
    }

    private void PlayRandomMusic()
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

    private IEnumerator FadeInMusic(AudioClip newTrack)
    {
        if (musicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic());
        }

        musicSource.clip = newTrack;
        musicSource.Play();
        musicSource.volume = 0f;

        float fadeDuration = 2f;
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

    private IEnumerator FadeOutMusic()
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

    private IEnumerator PlayNextTrackWithFade()
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

    public void SetCutoffFrequency(float fromValue, float toValue, float duration = 1f)
    {
        LeanTween.value(gameObject, fromValue, toValue, duration).setOnUpdate((float value) =>
        {
            mainMixer.SetFloat("lowpass", value);
        });
    }

    public void SetVolume(string parameterName, float volume)
    {
        mainMixer.SetFloat(parameterName, volume);
    }
}

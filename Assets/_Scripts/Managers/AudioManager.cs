using UnityEngine;
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
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Menu SFX Clips")]
    public List<MenuSFXClip> menuSFXClips;

    [Header("Music Tracks")]
    public List<AudioClip> musicTracks;

    private int lastTrackIndex = -1;

    public enum MenuSFX
    {
        Click,
        Error
    }

    private Dictionary<MenuSFX, AudioClip> menuSFXClipMap;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of AudioManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.RegisterAudioManager(this);
            InitializeMenuSFXClipMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Start playing background music
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

    // Method to play a specific menu SFX based on enum
    public void PlayMenuSFX(MenuSFX sfx)
    {
        if (menuSFXClipMap.TryGetValue(sfx, out AudioClip clip))
        {
            PlaySFX(clip);
        }
    }

    // Method to play a specific SFX
    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Method to play random music from the list with fade in/out
    private void PlayRandomMusic()
    {
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, musicTracks.Count);
        } while (randomIndex == lastTrackIndex);

        lastTrackIndex = randomIndex;
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
}
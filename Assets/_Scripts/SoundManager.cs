using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private Dictionary<string, AudioClip> soundDictionary;

    void Awake()
    {
        LoadSounds();
    }

    void LoadSounds()
    {
        soundDictionary = new Dictionary<string, AudioClip>();

        AudioClip[] sounds = Resources.LoadAll<AudioClip>("Sounds");

        foreach (AudioClip sound in sounds)
        {
            soundDictionary[sound.name] = sound;
        }
    }

    public AudioClip GetSound(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out AudioClip sound))
        {
            return sound;
        }
        else
        {
            Debug.LogWarning("Sound not found: " + soundName);
            return null;
        }
    }
}

using UnityEngine;

public class TestingSoundScript : MonoBehaviour
{
    [SerializeField] SoundManager soundManager;

    void Start()
    {

        // Play a sound
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        AudioClip clip = soundManager.GetSound("EnergyShield");
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}

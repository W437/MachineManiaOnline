using UnityEngine;
using Coffee.UIExtensions;

public class AttractionHandler : MonoBehaviour
{
    public GameObject coinTarget;
    public GameObject crystalTarget;
    public AudioClip coinAttractionSFX;
    public AudioClip crystalAttractionSFX;
    private AudioSource audioSource;
    private Vector3 coinOriginalScale;
    private Vector3 crystalOriginalScale;

    private void Start()
    {
        // Change audio source to the main SFX source of the mixer.
        audioSource = gameObject.AddComponent<AudioSource>();

        if (coinTarget != null)
        {
            coinOriginalScale = coinTarget.transform.localScale;
            var coinAttractor = coinTarget.GetComponent<UIParticleAttractor>();
            if (coinAttractor != null)
            {
                //coinAttractor.onAttracted.AddListener(OnCoinAttracted);
            }
        }

        if (crystalTarget != null)
        {
            crystalOriginalScale = crystalTarget.transform.localScale;
            var crystalAttractor = crystalTarget.GetComponent<UIParticleAttractor>();
            if (crystalAttractor != null)
            {
                //crystalAttractor.onAttracted.AddListener(OnCrystalAttracted);
            }
        }
    }

    private void OnCoinAttracted(GameObject attractedObject)
    {
        PlaySound(coinAttractionSFX);
        PlayPopUpEffect(coinTarget, coinOriginalScale);
    }

    private void OnCrystalAttracted(GameObject attractedObject)
    {
        PlaySound(crystalAttractionSFX);
        PlayPopUpEffect(crystalTarget, crystalOriginalScale);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayPopUpEffect(GameObject target, Vector3 originalScale)
    {
        LeanTween.scale(target, originalScale * 1.2f, 0.05f).setEase(LeanTweenType.easeOutElastic).setOnComplete(() =>
        {
            LeanTween.scale(target, originalScale, 0.01f).setEase(LeanTweenType.easeInElastic);
        });
    }
}

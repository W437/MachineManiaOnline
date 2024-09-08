using UnityEngine;
using Coffee.UIExtensions;

public class AttractionHandler : MonoBehaviour
{
    [SerializeField] GameObject coinTarget;
    [SerializeField] GameObject crystalTarget;
    [SerializeField] AudioClip coinAttractionSFX;
    [SerializeField] AudioClip crystalAttractionSFX;

    AudioSource _audioSource;
    Vector3 _coinOriginalScale;
    Vector3 _crystalOriginalScale;

    void Start()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();

        if (coinTarget != null)
        {
            _coinOriginalScale = coinTarget.transform.localScale;
            var coinAttractor = coinTarget.GetComponent<UIParticleAttractor>();
            if (coinAttractor != null)
            {
                //coinAttractor.onAttracted.AddListener(OnCoinAttracted);
            }
        }

        if (crystalTarget != null)
        {
            _crystalOriginalScale = crystalTarget.transform.localScale;
            var crystalAttractor = crystalTarget.GetComponent<UIParticleAttractor>();
            if (crystalAttractor != null)
            {
                //crystalAttractor.onAttracted.AddListener(OnCrystalAttracted);
            }
        }
    }

    void OnCoinAttracted(GameObject attractedObject)
    {
        PlaySound(coinAttractionSFX);
        PlayPopUpEffect(coinTarget, _coinOriginalScale);
    }

    void OnCrystalAttracted(GameObject attractedObject)
    {
        PlaySound(crystalAttractionSFX);
        PlayPopUpEffect(crystalTarget, _crystalOriginalScale);
    }

    void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    void PlayPopUpEffect(GameObject target, Vector3 originalScale)
    {
        LeanTween.scale(target, originalScale * 1.2f, 0.05f).setEase(LeanTweenType.easeOutElastic).setOnComplete(() =>
        {
            LeanTween.scale(target, originalScale, 0.01f).setEase(LeanTweenType.easeInElastic);
        });
    }
}

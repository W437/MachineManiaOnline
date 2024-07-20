using UnityEngine;
using Coffee.UIExtensions;

public class AttractionHandler : MonoBehaviour
{
    [SerializeField] private GameObject coinTarget;
    [SerializeField] private GameObject crystalTarget;
    [SerializeField] private AudioClip coinAttractionSFX;
    [SerializeField] private AudioClip crystalAttractionSFX;

    private AudioSource _audioSource;
    private Vector3 _coinOriginalScale;
    private Vector3 _crystalOriginalScale;

    private void Start()
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

    private void OnCoinAttracted(GameObject attractedObject)
    {
        PlaySound(coinAttractionSFX);
        PlayPopUpEffect(coinTarget, _coinOriginalScale);
    }

    private void OnCrystalAttracted(GameObject attractedObject)
    {
        PlaySound(crystalAttractionSFX);
        PlayPopUpEffect(crystalTarget, _crystalOriginalScale);
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
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

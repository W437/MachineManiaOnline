using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;
using System.Collections;

public class CurrencyEmitter : MonoBehaviour
{
    [SerializeField] Button emitCoinsButton;
    [SerializeField] Button emitCrystalsButton;

    [SerializeField] UIParticle coinUiParticle;
    [SerializeField] ParticleSystem coinParticleSystem;

    [SerializeField] UIParticle crystalUiParticle;
    [SerializeField] ParticleSystem crystalParticleSystem;

    int _coinEmitCount = 100;
    int _crystalEmitCount = 100;

    void Start()
    {
        if (emitCoinsButton != null)
        {
            emitCoinsButton.onClick.AddListener(EmitCoins);
        }

        if (emitCrystalsButton != null)
        {
            emitCrystalsButton.onClick.AddListener(EmitCrystals);
        }
    }

    void EmitCoins()
    {
        if (coinUiParticle != null && coinParticleSystem != null)
        {
            coinParticleSystem.Clear();
            StartCoroutine(EmitParticlesOverTime(coinParticleSystem, _coinEmitCount));
        }
    }

    void EmitCrystals()
    {
        if (crystalUiParticle != null && crystalParticleSystem != null)
        {
            crystalParticleSystem.Clear();
            StartCoroutine(EmitParticlesOverTime(crystalParticleSystem, _crystalEmitCount));
        }
    }

    IEnumerator EmitParticlesOverTime(ParticleSystem particleSystem, int totalParticles)
    {
        int emittedParticles = 0;
        while (emittedParticles < totalParticles)
        {
            particleSystem.Emit(1);
            emittedParticles++;
            yield return new WaitForSeconds(particleSystem.main.duration / totalParticles);
        }
    }
}

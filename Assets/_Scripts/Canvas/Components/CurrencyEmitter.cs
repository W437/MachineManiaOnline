using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;
using System.Collections;

public class CurrencyEmitter : MonoBehaviour
{
    [SerializeField] private Button emitCoinsButton;
    [SerializeField] private Button emitCrystalsButton;

    [SerializeField] private UIParticle coinUiParticle;
    [SerializeField] private ParticleSystem coinParticleSystem;

    [SerializeField] private UIParticle crystalUiParticle;
    [SerializeField] private ParticleSystem crystalParticleSystem;

    private int _coinEmitCount = 100;
    private int _crystalEmitCount = 100;

    private void Start()
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

    private void EmitCoins()
    {
        if (coinUiParticle != null && coinParticleSystem != null)
        {
            coinParticleSystem.Clear();
            StartCoroutine(EmitParticlesOverTime(coinParticleSystem, _coinEmitCount));
        }
    }

    private void EmitCrystals()
    {
        if (crystalUiParticle != null && crystalParticleSystem != null)
        {
            crystalParticleSystem.Clear();
            StartCoroutine(EmitParticlesOverTime(crystalParticleSystem, _crystalEmitCount));
        }
    }

    private IEnumerator EmitParticlesOverTime(ParticleSystem particleSystem, int totalParticles)
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

using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;
using System.Collections;

public class CurrencyEmitter : MonoBehaviour
{
    public Button emitCoinsButton;
    public Button emitCrystalsButton;

    public UIParticle coinUiParticle;
    public ParticleSystem coinParticleSystem;
    public int coinEmitCount = 100;

    public UIParticle crystalUiParticle;
    public ParticleSystem crystalParticleSystem;
    public int crystalEmitCount = 100;

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
            StartCoroutine(EmitParticlesOverTime(coinParticleSystem, coinEmitCount));
        }
    }

    private void EmitCrystals()
    {
        if (crystalUiParticle != null && crystalParticleSystem != null)
        {
            crystalParticleSystem.Clear();
            StartCoroutine(EmitParticlesOverTime(crystalParticleSystem, crystalEmitCount));
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

using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class CloakBehavior : NetworkBehaviour
{
    public float cloakDuration = 10f;
    PlayerController player;
    NetworkObject networkObject;
    [SerializeField] ParticleSystem pickupEffectPrefab;
    [SerializeField] AudioClip pickupSound;
    

    private void Awake()
    {
       
    }
    public void Initialize()
    {
        Debug.Log("Initializing Cloak");

        player = GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogError("PlayerController component is missing on the GameObject.");
            return;
        }

        networkObject = GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.LogError("NetworkObject component is missing on the GameObject.");
            return;
        }

        RPC_ActivateCloak(networkObject);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_ActivateCloak(NetworkObject networkObject)
    {
        Debug.Log("Cloaking");

        var playerObject = Runner.GetPlayerObject(networkObject.InputAuthority);
        if (playerObject == null)
        {
            Debug.LogError("Runner.GetPlayerObject returned null.");
            return;
        }

        player = playerObject.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogError("PlayerController component is missing on the player object.");
            return;
        }


        
        if (networkObject.HasInputAuthority)
        {
            Debug.Log("Has Input");
            if (pickupEffectPrefab != null)
            {
                ParticleSystem effectInstance = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                
                effectInstance.Play();
                AudioManager.Instance.PlayGameSFX(pickupSound);
                // Optionally, destroy the effect after its duration
                Destroy(effectInstance.gameObject, effectInstance.main.duration);
            }
            else
            {
                Debug.LogError("No particle effect prefab assigned!");
            }
            SetSpriteOpacity(0.5f);
        }
        else
        {
            if (pickupEffectPrefab != null)
            {
                ParticleSystem effectInstance = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

                effectInstance.Play();
                AudioManager.Instance.PlayGameSFX(pickupSound);
                // Optionally, destroy the effect after its duration
                Destroy(effectInstance.gameObject, effectInstance.main.duration);
            }
            else
            {
                Debug.LogError("No particle effect prefab assigned!");
            }
            SetSpriteOpacity(0f);
        }

        LeanTween.delayedCall(cloakDuration, () =>
        {
            RPC_DeactivateCloak(networkObject);
        });
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RPC_DeactivateCloak(NetworkObject network)
    {
        var playerObject = Runner.GetPlayerObject(network.InputAuthority);
        if (playerObject == null)
        {
            Debug.LogError("Runner.GetPlayerObject returned null during deactivation.");
            return;
        }

        player = playerObject.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogError("PlayerController component is missing on the player object during deactivation.");
            return;
        }

        if (pickupEffectPrefab != null)
        {
            ParticleSystem effectInstance = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

            effectInstance.Play();
            AudioManager.Instance.PlayGameSFX(pickupSound);
            // Optionally, destroy the effect after its duration
            Destroy(effectInstance.gameObject, effectInstance.main.duration);
        }
        else
        {
            Debug.LogError("No particle effect prefab assigned!");
        }
        SetSpriteOpacity(1f);
    }

    void SetSpriteOpacity(float opacity)
    {
        Debug.Log(player);
        foreach (var spriteRender in player.PlayerParts)
        {
            if (spriteRender != null)
            {
                Color color = spriteRender.color;
                color.a = opacity;
                spriteRender.color = color;
                Debug.Log($"Opacity set to: {color.a}");
            }
        }
    }
}

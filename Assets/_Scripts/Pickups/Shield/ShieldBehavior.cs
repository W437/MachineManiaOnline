using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class ShieldBehavior : NetworkBehaviour
{
    public float duration = 5f;
    private PlayerController player;
    private NetworkObject networkObject;
    [SerializeField] ParticleSystem shieldEffect;
    ParticleSystem effectInstance;
    [SerializeField]AudioClip AudioClip;


    public void Initialize()
    {

        Debug.Log("Initializing Shield");

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
        RPC_ActivateShield(networkObject);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_ActivateShield(NetworkObject net)
    {
        var playerObject = Runner.GetPlayerObject(net.InputAuthority);
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

        Debug.Log("About to be shielded");
        effectInstance = Instantiate(shieldEffect, player.transform);
       
        AudioManager.Instance.PlayGameSFX(AudioClip);
        player.IsShielded = true;
        shieldEffect.Play();
        LeanTween.delayedCall(duration, () =>
        {
            RPC_DeactivateSpeedBoost(net);
        });
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_DeactivateSpeedBoost(NetworkObject net )
    {
        var playerObject = Runner.GetPlayerObject(net.InputAuthority);
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
        if (net.HasInputAuthority)
        {

            if (player.IsShielded)
            {
                AudioManager.Instance.StopGameSFX();
                player.IsShielded = false;
                Destroy(effectInstance.gameObject);
                Debug.Log("Not Shielded anymore");
            }
        }

    }
}

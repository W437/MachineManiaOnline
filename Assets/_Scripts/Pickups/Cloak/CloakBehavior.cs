using Fusion;
using UnityEngine;

public class CloakBehavior : NetworkBehaviour
{
    public float cloakDuration = 10f;
    PlayerController player;
    NetworkObject networkObject;

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
            // local player
            SetSpriteOpacity(0.5f);
        }
        else
        {
            //all other players
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

using Fusion;
using UnityEngine;

public class CloakBehavior : NetworkBehaviour
{
    public float cloakDuration = 10f;
    private PlayerController player;
    private NetworkObject networkObject;

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

        RPC_ActivateCloak(networkObject.InputAuthority);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_ActivateCloak(PlayerRef playerRef)
    {
        Debug.Log("Cloaking");

        var playerObject = Runner.GetPlayerObject(playerRef);
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

        var spriteRenderer = player.transform.Find("Visual/Sprite")?.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on player.");
            return;
        }

        if (networkObject.InputAuthority == playerRef)
        {
            // This is the local player, set their opacity to 0.5
            SetSpriteOpacity(spriteRenderer, 0.5f);
        }
        else
        {
            // For all other players, set the opacity to 0 (invisible)
            SetSpriteOpacity(spriteRenderer, 0f);
        }

        LeanTween.delayedCall(cloakDuration, () =>
        {
            RPC_DeactivateCloak(playerRef);
        });
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_DeactivateCloak(PlayerRef playerRef)
    {
        var playerObject = Runner.GetPlayerObject(playerRef);
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

        var spriteRenderer = player.transform.Find("Visual/Sprite")?.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on player during deactivation.");
            return;
        }

        // Restore full visibility to all players
        SetSpriteOpacity(spriteRenderer, 1f);
    }

    private void SetSpriteOpacity(SpriteRenderer spriteRenderer, float opacity)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = opacity;
            spriteRenderer.color = color;
            Debug.Log($"Opacity set to: {color.a}");
        }
        else
        {
            Debug.LogError("SpriteRenderer is null when trying to set opacity.");
        }
    }
}

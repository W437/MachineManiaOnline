using Fusion;
using UnityEngine;

public class CloakBehavior : NetworkBehaviour
{
    public float cloakDuration = 10f;
    private PlayerController player;
    private NetworkObject networkObject;

    public void Initialize()
    {
        player = GetComponentInParent<PlayerController>();
        networkObject = player.GetComponent<NetworkObject>();
        if (player != null && networkObject != null)
        {
            RPC_ActivateCloak(networkObject.InputAuthority);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_ActivateCloak(PlayerRef playerRef)
    {
        player = Runner.GetPlayerObject(playerRef).GetComponent<PlayerController>();
        var spriteRenderer = player.transform.Find("Visual/Sprite").GetComponent<SpriteRenderer>();

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
        player = Runner.GetPlayerObject(playerRef).GetComponent<PlayerController>();
        var spriteRenderer = player.transform.Find("Visual/Sprite").GetComponent<SpriteRenderer>();

        // Restore full visibility to all players
        SetSpriteOpacity(spriteRenderer, 1f);
        Runner.Despawn(Object);
    }

    private void SetSpriteOpacity(SpriteRenderer spriteRenderer, float opacity)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = opacity;
            spriteRenderer.color = color;
        }
    }
}

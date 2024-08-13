using Fusion;
using UnityEngine;

public class ShieldBehavior : NetworkBehaviour
{
    public float duration = 10f;
    private PlayerController player;
    private NetworkObject networkObject;

    public void Initialize()
    {
        player = GetComponentInParent<PlayerController>();
        networkObject = player.GetComponent<NetworkObject>();
        if (player != null && networkObject != null)
        {
            RPC_ActivateShield(networkObject.InputAuthority);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ActivateShield(PlayerRef playerRef)
    {
        player = Runner.GetPlayerObject(playerRef).GetComponent<PlayerController>();
        //player.isInvincible = true;

        LeanTween.delayedCall(duration, () =>
        {
            RPC_DeactivateShield(playerRef);
        });
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_DeactivateShield(PlayerRef playerRef)
    {
        player = Runner.GetPlayerObject(playerRef).GetComponent<PlayerController>();
        //player.isInvincible = false;
        Runner.Despawn(GetComponent<NetworkObject>());
    }
}

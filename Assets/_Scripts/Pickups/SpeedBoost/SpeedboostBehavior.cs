using Fusion;
using UnityEngine;

public class SpeedBoostBehavior : NetworkBehaviour
{
    public float boostMultiplier = 2f;
    public float duration = 5f;
    private PlayerController player;
    private NetworkObject networkObject;

    public void Initialize()
    {
        player = GetComponentInParent<PlayerController>();
        networkObject = player.GetComponent<NetworkObject>();
        if (player != null && networkObject != null)
        {
            RPC_ActivateSpeedBoost(networkObject.InputAuthority);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ActivateSpeedBoost(PlayerRef playerRef)
    {
        player = Runner.GetPlayerObject(playerRef).GetComponent<PlayerController>();
        //player.movementSpeed *= boostMultiplier;

        LeanTween.delayedCall(duration, () =>
        {
            RPC_DeactivateSpeedBoost(playerRef);
        });
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_DeactivateSpeedBoost(PlayerRef playerRef)
    {
        player = Runner.GetPlayerObject(playerRef).GetComponent<PlayerController>();
        //player.movementSpeed /= boostMultiplier;
        Runner.Despawn(GetComponent<NetworkObject>());
    }
}

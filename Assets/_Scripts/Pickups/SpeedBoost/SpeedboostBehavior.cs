using Fusion;
using UnityEngine;

public class SpeedBoostBehavior : NetworkBehaviour
{
    public float boostMultiplier = 1f;
    public float speedMulti = 3f;
    public float duration = 20f;
    private PlayerController player;
    private NetworkObject networkObject;
   

    public void Initialize()
    {
       
        Debug.Log("Initializing Speed");

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
        RPC_ActivateSpeedBoost(networkObject);
    }

    [Rpc(RpcSources.InputAuthority,RpcTargets.All)]
    private void RPC_ActivateSpeedBoost(NetworkObject net)
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
        Debug.Log(playerObject.Runner.LocalPlayer);

        float originalSpeedMulit = player.Stats.MaxSpeed;
        float originalAccelMulti = player.Stats.Acceleration;

        float newSpeedMulit = originalSpeedMulit * speedMulti;
        float newAccelMulti = originalAccelMulti * boostMultiplier;
        if (net.HasInputAuthority)
        {
            Debug.Log("Has Input");
            //This is the local player, set their opcity to 0.5
            //Half works might be clamped 
            player.ChangeSpeedAndAcceleration(newSpeedMulit, newAccelMulti);
        }
        


      

        LeanTween.delayedCall(duration, () =>
        {
            RPC_DeactivateSpeedBoost(net,originalSpeedMulit,originalAccelMulti);
        });
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_DeactivateSpeedBoost(NetworkObject net,float originalSpeed,float originalAceelertion)
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
        player.ChangeSpeedAndAcceleration(originalSpeed,originalAceelertion);

        }
        
    }
}

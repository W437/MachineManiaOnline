using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockBehavior : NetworkBehaviour
{

    private PlayerController player;
    private NetworkObject networkObject;
    [SerializeField] private AudioClip pickupSound;
    private float deathDuration=3;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

    }

    public void Init()
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
        RPC_Zap(networkObject);


    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
	public void RPC_Zap(NetworkObject networkObject)
	{
        Debug.Log("ZAPPING");
        var playerObject = Runner.GetPlayerObject(networkObject.InputAuthority);
        if (playerObject == null)
        {
            Debug.LogError("Runner.GetPlayerObject returned null.");
            return;
        }

       PlayerController player = playerObject.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogError("PlayerController component is missing on the player object.");
            return;
        }
        AudioManager.Instance.PlayGameSFX(pickupSound);
        if (networkObject.HasInputAuthority)
        {
            Debug.Log("You are not supposed to be zapped");
            Debug.Log(networkObject.Runner.LocalPlayer);


        }
        else
        {
            NetworkObject newP = Runner.GetPlayerObject(networkObject.Runner.LocalPlayer);
            if (newP.TryGetComponent(out PlayerController pc))
            {
                StartCoroutine(Zap(pc));
            }
            
        }
        
    }
    private IEnumerator Zap(PlayerController playerController)
    {

        if (playerController.IsShielded)
        {
            Debug.Log("Shielded not damaged");
            
            yield return null;
        }
       
        playerController.TogglePlayerMovement(false);
        yield return new WaitForSeconds(deathDuration);
        playerController.TogglePlayerMovement(true);

    }
}

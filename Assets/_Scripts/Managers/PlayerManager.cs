using Fusion;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [Networked] public string net_PlayerName { get; set; }
    [Networked] public int net_Score { get; set; }
    [Networked] public int net_PlayerID { get; set; }
    [Networked] public bool net_CanMove { get; set; }
    [Networked] public bool net_IsAlive { get; set; }

    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            net_CanMove = false; 
            net_IsAlive = true; 
        }
    }

    public void SetPlayerName(string name)
    {
        if (HasStateAuthority)
        {
            net_PlayerName = name;
        }
    }

    public void AddScore(int score)
    {
        if (HasStateAuthority)
        {
            net_Score += score;
        }
    }

    public void KillPlayer()
    {
        if (HasStateAuthority)
        {
            net_IsAlive = false;
            RpcPlayerKilled();
        }
    }

    public void RespawnPlayer(Vector3 position)
    {
        if (HasStateAuthority)
        {
            net_IsAlive = true;
            transform.position = position;
            RpcPlayerRespawned(position);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcPlayerKilled()
    {
        // Handle player killed logic on all clients
        //playerController.DisablePlayer();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcPlayerRespawned(Vector3 position)
    {
        // Handle player respawn logic on all clients
        //playerController.EnablePlayer();
        transform.position = position;
    }
}

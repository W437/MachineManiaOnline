using Fusion;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [Networked] public string PlayerName { get; set; }
    [Networked] public int Score { get; set; }
    [Networked] public int PlayerID { get; set; }
    [Networked] public bool CanMove { get; set; }
    [Networked] public bool IsAlive { get; set; }

    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            CanMove = false; 
            IsAlive = true; 
        }
    }

    public void SetPlayerName(string name)
    {
        if (HasStateAuthority)
        {
            PlayerName = name;
        }
    }

    public void AddScore(int score)
    {
        if (HasStateAuthority)
        {
            Score += score;
        }
    }

    public void KillPlayer()
    {
        if (HasStateAuthority)
        {
            IsAlive = false;
            RpcPlayerKilled();
        }
    }

    public void RespawnPlayer(Vector3 position)
    {
        if (HasStateAuthority)
        {
            IsAlive = true;
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

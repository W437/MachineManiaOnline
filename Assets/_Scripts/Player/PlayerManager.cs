using Fusion;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [Networked] public string net_PlayerName { get; set; }
    [Networked] public int net_Score { get; set; }
    [Networked] public int net_PlayerID { get; set; }
    [Networked] public bool net_IsAlive { get; set; }
    [Networked] public bool net_IsReady { get; set; }

    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
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

    public void SetPlayerReady(bool ready)
    {
        net_IsReady = ready;
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
        transform.position = position;
    }

    private void SavePlayerData()
    {
        PlayerPrefs.SetString("PlayerName", net_PlayerName);
        PlayerPrefs.SetInt("PlayerScore", net_Score);
        PlayerPrefs.SetInt("PlayerID", net_PlayerID);
        PlayerPrefs.SetInt("IsAlive", net_IsAlive ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadPlayerData()
    {
        net_PlayerName = PlayerPrefs.GetString("PlayerName", "DefaultName");
        net_Score = PlayerPrefs.GetInt("PlayerScore", 0);
        net_PlayerID = PlayerPrefs.GetInt("PlayerID", 0);
        net_IsAlive = PlayerPrefs.GetInt("IsAlive", 1) == 1;
    }
}

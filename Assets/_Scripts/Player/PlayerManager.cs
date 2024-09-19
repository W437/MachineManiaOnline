using Fusion;
using System.Collections;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class PlayerManager : NetworkBehaviour
{
    // Player Stats
    [Networked] public string PlayerName { get; set; }
    [Networked] public int Level { get; set; }
    [Networked] public int Gold { get; set; }
    [Networked] public int Diamonds { get; set; }
    [Networked] public int PlayerID { get; set; }

    [Networked] int _isAliveInt { get; set; }
    [Networked] int _isReadyInt { get; set; }

    SpriteRenderer spriteRenderer;
    Vector3 respawnPosition;

    // public lobby hub
    const float HUB_MSG_COOLDOWN_DURATION = 2f;
    float lastMessageTime;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            ApplySavedStats();
        }

        IsAlive = true;
        IsReady = false;
    }

    public void ApplySavedStats()
    {
        PlayerData playerData = PlayerData.Instance;
        if (playerData != null)
        {
            PlayerName = playerData.PlayerName;
            Level = playerData.Level;
            Gold = playerData.Gold;
            Diamonds = playerData.Diamonds;

            Debug.Log($"Player {PlayerName} has been spawned with: Level: {Level} " +
                      $"Gold: {Gold}, Diamonds: {Diamonds}");
        }
        else
        {
            Debug.LogError("PlayerData instance is null.");
        }
    }

    public void Kill()
    {
        if (!IsAlive) return;
        IsAlive = false;

        GetComponent<PlayerController>().CanMove = false;
        spriteRenderer.enabled = false;

        respawnPosition = transform.position;

        RpcOnPlayerKilled();
    }

    public bool CanInteract()
    {
        return Time.time - lastMessageTime >= HUB_MSG_COOLDOWN_DURATION;
    }
    
    public void UpdateCooldown()
    {
        lastMessageTime = Time.time;
    }
    
    public void AddScore(int points)
    {
        if (HasStateAuthority)
        {
            Level += points;
            RpcUpdateScore(Level);
        }
    }
    
    public bool IsAlive
    {
        get => _isAliveInt > 0;
        set => _isAliveInt = value ? _isAliveInt + 1 : -(_isAliveInt + 1);
    }

    public bool IsReady
    {
        get => _isReadyInt > 0;
        set => _isReadyInt = value ? _isReadyInt + 1 : -(_isReadyInt + 1);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcOnPlayerKilled()
    {
        spriteRenderer.enabled = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RpcUpdateScore(int newScore)
    {
        // Update the player's score UI or other logic
    }
}

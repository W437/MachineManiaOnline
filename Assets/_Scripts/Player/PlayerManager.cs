using Fusion;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Networked] public string net_PlayerName { get; set; }
    [Networked] public int net_Score { get; set; }
    [Networked] public int net_PlayerID { get; set; }
    [Networked] public bool net_IsAlive { get; set; }
    [Networked] public bool net_IsReady { get; set; }

    private const float HUB_MSG_COOLDOWN_DURATION = 2f;
    private float lastEmoteTime;
    private float lastMessageTime;

    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    public void UpdateCooldown()
    {
        lastMessageTime = Time.time;
    }

    public bool CanInteract()
    {
        return Time.time - lastMessageTime >= HUB_MSG_COOLDOWN_DURATION;
    }

}

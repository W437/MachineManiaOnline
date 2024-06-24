using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using Cinemachine;
using System.Linq;

public class M_Player : NetworkBehaviour
{
    public Dictionary<int, bool> playerReadyStates = new Dictionary<int, bool> { };

    private M_Network M_Network;
    public bool IsSpawned = false;

    private void Awake()
    {
        ServiceLocator.RegisterPlayerManager(this);
        M_Network = ServiceLocator.GetNetworkManager();
    }

    public override void Spawned()
    {
        Debug.Log("Spawned called");
        IsSpawned = true;
    }

    public void SetReady(int key, bool value)
    {
        if(!playerReadyStates.ContainsKey(key))
        {
            playerReadyStates.Add(key, value);
        }
    }

    public void RemoveReady(int key)
    {
        playerReadyStates.Remove(key);
    }

    public bool ContainsKey(int key)
    {
        return playerReadyStates.ContainsKey(key);
    }

    /*    public Dictionary<int, bool> GetReadyStates()
        {
            Dictionary<int, bool> readyStates = new Dictionary<int, bool>();

            foreach (var kvp in playerReadyStates)
            {
                readyStates[kvp.Key] = kvp.Value;
            }

            return readyStates;
        }*/

}

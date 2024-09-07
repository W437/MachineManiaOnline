using Fusion;
using UnityEngine;

[System.Serializable]
public struct PublicLobbyPosition
{
    public Transform Position;
    public bool IsOccupied;
    public PlayerRef PlayerRef;
}
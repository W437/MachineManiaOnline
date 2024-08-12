using UnityEngine;

public class PrivateLobbyPosition
{
    public Transform Position { get; set; }
    public bool IsOccupied { get; set; }

    public PrivateLobbyPosition(Transform position)
    {
        Position = position;
        IsOccupied = false;
    }
}

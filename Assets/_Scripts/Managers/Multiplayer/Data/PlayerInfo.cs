
using Fusion;

public struct PlayerInfo : INetworkStruct
{
    public PlayerRef net_Player;
    public NetworkString<_32> net_PlayerName;
    public NetworkString<_32> net_PlayerState;
    public NetworkString<_32> net_PlayerMessage;
}
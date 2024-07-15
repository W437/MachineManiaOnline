
using Fusion;

public struct PlayerInfo : INetworkStruct
{
    public PlayerRef Player;
    public NetworkString<_32> PlayerName;
    public NetworkString<_32> PlayerState;
    public NetworkString<_32> PlayerMessage;
}
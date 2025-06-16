using Fusion;

public struct NetworkInputData : INetworkInput
{
    public byte playerId; // 1 for Player 1 (left helix), 2 for Player 2 (right helix)
    public float rotationDeltaX; // Rotation input for the helix (based on keyboard input)
}
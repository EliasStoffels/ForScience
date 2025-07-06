using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    private Dictionary<ulong, PlayerIdentifier> steamIdToPlayer = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterPlayer(PlayerIdentifier player)
    {
        ulong steamId = player.SteamId.Value;
        if (!steamIdToPlayer.ContainsKey(steamId))
        {
            steamIdToPlayer.Add(steamId, player);
        }
    }

    public void UnregisterPlayer(PlayerIdentifier player)
    {
        ulong steamId = player.SteamId.Value;
        steamIdToPlayer.Remove(steamId);
    }

    public PlayerIdentifier GetPlayerBySteamId(ulong steamId)
    {
        return steamIdToPlayer.TryGetValue(steamId, out var player) ? player : null;
    }

    public PlayerIdentifier GetLocalPlayer()
    {
        ulong mySteamId = SteamClient.SteamId.Value;
        return GetPlayerBySteamId(mySteamId);
    }
}

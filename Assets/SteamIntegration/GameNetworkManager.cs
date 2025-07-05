using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class GameNetworkManager : MonoBehaviour
{
    public static GameNetworkManager instance { get; private set; } = null;

    private FacepunchTransport  m_Transport = null;

    public Lobby? currentLoby = null;

    public ulong hostId;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        m_Transport = GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += SteamMatchmaking_OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += SteamMatchmaking_OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;
    }

    private void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= SteamMatchmaking_OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= SteamMatchmaking_OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= SteamFriends_OnGameLobbyJoinRequested;

        if(NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
    }

    private void OnApplicationQuit()
    {
        Disconnected();
    }

    //accpet invite or join friend
    private async void SteamFriends_OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        RoomEnter joinedLobby = await lobby.Join();
        if(joinedLobby != RoomEnter.Success)
        {
            Debug.Log("failed to create lobby");
        }
        else
        {
            currentLoby = lobby;
            Debug.Log("joined lobby");
        }
    }

    private void SteamMatchmaking_OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId)
    {
        Debug.Log("Lobby Created");
    }

    private void SteamMatchmaking_OnLobbyInvite(Friend friend, Lobby lobby)
    {
        Debug.Log($"Invite from {friend.Name}");
    }

    private void SteamMatchmaking_OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} left");
    }

    private void SteamMatchmaking_OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} joined");
    }

    private void SteamMatchmaking_OnLobbyEntered(Lobby lobby)
    {
        if(NetworkManager.Singleton.IsHost)
        {
            return;
        }
        StartClient(currentLoby.Value.Owner.Id);
    }

    private void SteamMatchmaking_OnLobbyCreated(Result result, Lobby lobby)
    {
        if(result != Result.OK)
        {
            Debug.Log("lobby was not created");
            return;
        }
        else
        {
            lobby.SetPublic();
            lobby.SetJoinable(true);
            lobby.SetGameServer(lobby.Owner.Id);
        }
        Debug.Log("lobby created");
    }

    public async void StartHost(int maxNumbers)
    {
        NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
        NetworkManager.Singleton.StartHost();
        currentLoby = await SteamMatchmaking.CreateLobbyAsync(maxNumbers);
    }

    public void StartClient(SteamId steamId)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
        m_Transport.targetSteamId = steamId;
        if(NetworkManager.Singleton.StartClient() )
        {
            Debug.Log("client has started");
        }
    }

    public void Disconnected()
    {
        currentLoby?.Leave();
        if(NetworkManager.Singleton == null )
        {
            return;
        }
        if(NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
        }
        else
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        }
        NetworkManager.Singleton.Shutdown(true);
        Debug.Log("disconnected");
    }

    private void Singleton_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected!");
    }

    private void Singleton_OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected!");
    }

    private void Singleton_OnServerStarted()
    {
        Debug.Log("host started");
    }
}

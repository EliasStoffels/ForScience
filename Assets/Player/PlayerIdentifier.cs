using Unity.Netcode;
using Steamworks;
using Unity.Collections;
using UnityEngine;
using TMPro;

public class PlayerIdentifier : NetworkBehaviour
{
    public NetworkVariable<ulong> SteamId = new NetworkVariable<ulong>(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString128Bytes> playerName = new(writePerm: NetworkVariableWritePermission.Owner);

    [SerializeField] private TextMeshPro m_NameTag;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SubmitSteamIdServerRpc(SteamClient.SteamId.Value);
            playerName.Value = SteamClient.Name;
            m_NameTag.text = playerName.Value.ToString();
        }

        PlayerManager.Instance.RegisterPlayer(this);
    }

    public override void OnNetworkDespawn()
    {
        PlayerManager.Instance?.UnregisterPlayer(this);
    }

    [ServerRpc]
    private void SubmitSteamIdServerRpc(ulong id)
    {
        SteamId.Value = id;
    }
}

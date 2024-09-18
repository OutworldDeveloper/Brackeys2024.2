using System.Collections.Generic;
using System.Text;
using System;
using Unity.Netcode;
using UnityEngine;

public sealed class Multiplayer : PersistentSingleton<Multiplayer>
{
    public static HostSession HostSession { get; private set; }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public static bool StartHost()
    {        
        HostSession = new HostSession();
        var clientId = NetworkManager.Singleton.LocalClientId;
        HostSession.AddPlayer(clientId, new PlayerData(clientId, Name.Mine));

        bool isSuccess = NetworkManager.Singleton.StartHost();

        if (isSuccess == false)
        {
            Debug.LogWarning("Couldn't start host");
            return false;
        }

        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        return true;
    }

    public static bool StartClient()
    {
        var payload = new ConnectionPayload(Name.Mine);
        //NetworkManager.Singleton.NetworkConfig.ConnectionData = payload.ToBytes();
        NetworkManager.Singleton.NetworkConfig.ConnectionData = ASCIIEncoding.ASCII.GetBytes(Name.Mine);
        bool isSuccess = NetworkManager.Singleton.StartClient();

        if (isSuccess == false)
        {
            Debug.LogWarning("Couldn't start client");
            return false;
        }

        return true;
    }

    private static void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request, 
        NetworkManager.ConnectionApprovalResponse response)
    {
        //var payload = ConnectionPayload.FromBytes(request.Payload);

        response.Approved = NetworkManager.Singleton.ConnectedClientsList.Count + 1 < Lobby.MAX_PLAYERS; 

        if (response.Approved == false)
            return;

        HostSession.AddPlayer(request.ClientNetworkId, new PlayerData(request.ClientNetworkId, 
            ASCIIEncoding.ASCII.GetString(request.Payload)));
    }

    private void OnClientDisconnected(ulong id)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            HostSession.RemovePlayer(id);
        }
    }

}

public sealed class HostSession
{

    private readonly Dictionary<ulong, PlayerData> _playerData = new Dictionary<ulong, PlayerData>();

    public void AddPlayer(ulong clientId, PlayerData playerData)
    {
        _playerData.Add(clientId, playerData);
    }

    public PlayerData GetPlayer(ulong clientId)
    {
        if (_playerData.ContainsKey(clientId))
            return _playerData[clientId];

        throw new Exception("Requested clientId is not present in the dictionary");
    }

    public void RemovePlayer(ulong clientId)
    {
        _playerData.Remove(clientId);
    }

}

public sealed class PlayerData
{

    public readonly ulong ClientId;
    public readonly string Name;

    public PlayerData(ulong clientId, string name)
    {
        ClientId = clientId;
        Name = name;
    }

}

[Serializable]
public sealed class ConnectionPayload
{

    public string PlayerName { get; private set; }

    public ConnectionPayload(string playerName)
    {
        PlayerName = playerName;
    }

    public byte[] ToBytes()
    {
        string json = JsonUtility.ToJson(this);
        return Encoding.ASCII.GetBytes(json);
    }

    public static ConnectionPayload FromBytes(byte[] bytes)
    {
        string json = Encoding.ASCII.GetString(bytes);
        Debug.Log(json);
        return JsonUtility.FromJson<ConnectionPayload>(json);
    }

}

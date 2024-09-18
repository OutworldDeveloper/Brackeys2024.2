using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public sealed class Lobby : NetworkBehaviour
{

    public const int MAX_PLAYERS = 4;

    public event Action<PlayerLobbyState[]> Updated;

    private NetworkList<PlayerLobbyState> _lobbyPlayers;

    private void OnEnable()
    {
        _lobbyPlayers = new NetworkList<PlayerLobbyState>();
    }

    private void OnDisable()
    {
        _lobbyPlayers.Dispose();
        _lobbyPlayers.OnListChanged -= OnReadyStateChanged;

        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient == true)
        {
            _lobbyPlayers.OnListChanged += OnReadyStateChanged;
        }

        if (IsServer == true)
        {
            //foreach (var id in NetworkManager.ConnectedClientsIds)
            //{
            //    OnClientConnected(id);
            //}
        }

        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public bool IsEveryoneReady()
    {
        if (_lobbyPlayers.Count < MAX_PLAYERS)
            return false;

        foreach (var player in _lobbyPlayers)
        {
            if (player.IsReady == false)
                return false;
        }

        return true;
    }

    public bool IsLocalClientReady()
    {
        foreach (var player in _lobbyPlayers)
        {
            if (player.ClientId != NetworkManager.LocalClientId)
                continue;

            return player.IsReady;
        }

        return false;
    }

    public PlayerLobbyState[] GetPlayers()
    {
        var array = new PlayerLobbyState[_lobbyPlayers.Count];

        for (int i = 0; i < _lobbyPlayers.Count; i++)
        {
            array[i] = _lobbyPlayers[i];
        }

        return array;
    }

    public void StartGame()
    {
        if (IsHost == false)
            return;

    }

    private void OnReadyStateChanged(NetworkListEvent<PlayerLobbyState> changeEvent)
    {
        Updated?.Invoke(GetPlayers());
    }

    private void OnClientConnected(ulong id)
    {
        var playerData = Multiplayer.HostSession.GetPlayer(id);
        Notification.Show($"{playerData.Name} connected!");
        _lobbyPlayers.Add(new PlayerLobbyState(playerData, false));
        Updated?.Invoke(GetPlayers());
    }

    private void OnClientDisconnected(ulong id)
    {
        for (int i = 0; i < _lobbyPlayers.Count; i++)
        {
            var lobbyPlayer = _lobbyPlayers[i];

            if (lobbyPlayer.ClientId != id)
                continue;

            _lobbyPlayers.RemoveAt(i);
        }

        Updated?.Invoke(GetPlayers());
    }

    public void SetReadyStatus(bool status)
    {
        SetReadyStatusServerRpc(status);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyStatusServerRpc(bool status, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        Debug.Log($"Server received ReadyStatus of {senderId}, status: {status}");

        for (int i = 0; i < _lobbyPlayers.Count; i++)
        {
            var lobbyPlayer = _lobbyPlayers[i];

            if (lobbyPlayer.ClientId != senderId)
                continue;

            _lobbyPlayers[i] = status ? lobbyPlayer.Ready() : lobbyPlayer.NotReady();
        }

        // Not bad option at all
        //if (status == true && _players.Contains(senderId) == false)
        //{
        //    //_players.Add(clientId);
        //}
        //
        //if (status == false && _players.Contains(senderId) == true)
        //{
        //    //_players.Remove(clientId);
        //}
    }

}

public struct PlayerLobbyState : INetworkSerializable, IEquatable<PlayerLobbyState>
{

    public ulong ClientId;
    public FixedString32Bytes Name;
    public bool IsReady;

    public PlayerLobbyState(ulong clientId, string name, bool isReady)
    {
        ClientId = clientId;
        Name = name;
        IsReady = isReady;
    }

    public PlayerLobbyState(ulong clientId, FixedString32Bytes name, bool isReady)
    {
        ClientId = clientId;
        Name = name;
        IsReady = isReady;
    }

    public PlayerLobbyState(PlayerData playerData, bool isReady) : this(playerData.ClientId, playerData.Name, isReady) { }

    public readonly PlayerLobbyState Ready()
    {
        return new PlayerLobbyState(ClientId, Name, true);
    }

    public readonly PlayerLobbyState NotReady()
    {
        return new PlayerLobbyState(ClientId, Name, false);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref IsReady);
    }

    public readonly bool Equals(PlayerLobbyState other)
    {
        return ClientId == other.ClientId && Name == other.Name && IsReady == other.IsReady;
    }

}

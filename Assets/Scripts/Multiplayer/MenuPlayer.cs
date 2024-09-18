using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public sealed class MenuPlayer : BasePlayer
{

    [SerializeField] private Lobby _lobby;
    [SerializeField] private Prefab<UI_LobbyWindow> _lobbyWindow;

    private Option<UI_LobbyWindow> _currentLobbyWindow;

    protected override void Start()
    {
        base.Start();

        OpenPanel(Panels.GenericMenu).
            WithLabel($"You are {Name.Mine}").
            WithButton("Start Host", StartHost).
            WithButton("Start Client", StartClient);
    }

    private void StartHost()
    {
        if (Multiplayer.StartHost())
        {
            OpenPanel(_lobbyWindow).Setup(_lobby);
        }
    }

    private void StartClient()
    {
        if (Multiplayer.StartClient())
        {
            OpenPanel(_lobbyWindow).Setup(_lobby);
        }
    }

    private void OnClientConnected(ulong id)
    {
        if (NetworkManager.Singleton.LocalClientId != id)
        {
            Notification.Show($"A client {id} disconneted!");
            return;
        }

        _currentLobbyWindow = Option<UI_LobbyWindow>.Some(OpenPanel(_lobbyWindow).Setup(_lobby));
    }

    private void OnClientDisconnected(ulong id)
    {
        if (NetworkManager.Singleton.LocalClientId != id)
        {
            Notification.Show($"A client {id} disconneted!");
            return;
        }

        _currentLobbyWindow.Do(window => window.CloseAndDestroy());
    }

}

using System.Collections;
using TMPro;
using UnityEngine;

public sealed class UI_LobbyWindow : UI_Panel
{

    [SerializeField] private UI_PlayerCard[] _playerCards;
    [SerializeField] private GameObject _hostVisuals;

    private Lobby _lobby;

    public UI_LobbyWindow Setup(Lobby lobby)
    {
        _lobby = lobby;
        _lobby.Updated += Refresh;
        Refresh(lobby.GetPlayers());

        _hostVisuals.SetActive(lobby.IsHost);

        return this;
    }

    private void OnDestroy()
    {
        _lobby.Updated -= Refresh;
    }

    private void Refresh(PlayerLobbyState[] states)
    {
        for (int i = 0; i < _playerCards.Length; i++)
        {
            var card = _playerCards[i];

            if (i > states.Length - 1)
            {
                card.Hide();
                continue;
            }

            var state = states[i];
            card.Show(state.Name.ToString(), state.IsReady);
        }
    }

    public override bool CanRemoveAtWill() => false;

    public void ButtonStart()
    {
        _lobby.StartGame();
    }

    public void ButtonReady()
    {
        Debug.Log("Ready button pressed");
        _lobby.SetReadyStatus(!_lobby.IsLocalClientReady());
    }

    public void ButtonClose()
    {

    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Finale : MonoBehaviour
{
    
    [SerializeField] private ItemPedistal[] _pedistals;
    [SerializeField] private Door _finalDoor;
    [SerializeField] private GameObject _finaleScene;
    [SerializeField] private PlayerTrigger _finaleTrigger;

    private void Awake()
    {
        foreach (var pedistal in _pedistals)
        {
            pedistal.Updated += OnPedistalUpdated;
        }

        _finaleTrigger.EnterEvent.AddListener(OnPlayerEnterFinale);
    }

    private void Start()
    {
        _finalDoor.Block();

        _finaleScene.SetActive(false);
    }

    private void OnPedistalUpdated()
    {
        foreach (var pedistal in _pedistals)
        {
            if (pedistal.DisplayItem == null)
                return;
        }

        _finalDoor.Unblock();
        _finalDoor.Open();

        _finaleScene.SetActive(true);
    }

    private void OnPlayerEnterFinale(PlayerCharacter player)
    {
        player.ApplyModifier(new FinaleBlockModifier(), 5f);
        Notification.Show("Thank you for playing!", 1.8f);
        Delayed.Do(() => OpenThxScreen(player.Player), 2f);
    }

    private void OpenThxScreen(BasePlayer player)
    {
        player.OpenPanel(Panels.GenericMenu).
            WithLabel("Finish").
            WithDescription("Thank you for playing and completing our game!").
            WithButton("Ok", () => SceneManager.LoadScene(SceneManager.GetActiveScene().name)).
            WithClosability(false);
    }

    private sealed class FinaleBlockModifier : CharacterModifier
    {

        public override bool CanCrouch()
        {
            return false;
        }

        public override bool CanRotateCamera()
        {
            return false;
        }

        public override bool CanInteract()
        {
            return false;
        }

        public override bool CanJump()
        {
            return false;
        }

        public override float GetSpeedMultiplier()
        {
            return 0f;
        }

    }

}

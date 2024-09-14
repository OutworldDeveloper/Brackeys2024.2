using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesPuzzle : MonoBehaviour
{

    [SerializeField] private Plate[] _plates = new Plate[0];
    [SerializeField] private MoviePawn _victoryPawn;
    [SerializeField] private Player _player;
    [SerializeField] private Cage _cage;

    private bool _isBeaten = false;

    private void Awake()
    {
        foreach (var plate in _plates)
        {
            plate.Pedistal.ItemPlaced += OnItemAdded;
            plate.Pedistal.ItemRemoved += OnItemRemoved;
        }
    }

    private void OnItemAdded(Item item)
    {
        CheckVictory();
    }

    private void OnItemRemoved()
    {
        CheckVictory();
    }

    private void CheckVictory()
    {
        if (_isBeaten)
            return;

        if (ArePlatesServed() == false)
            return;

        _isBeaten = true;

        foreach (var plate in _plates)
            plate.Interaction.Lock();

        _player.AddPawn(_victoryPawn);

        Delayed.Do(() =>
        {
            _cage.Open();
        }, 0.2f);
    }

    private bool ArePlatesServed()
    {
        foreach (var plate in _plates)
        {
            if (plate.Pedistal.ContainsItem == false)
                return false;

            if (plate.Pedistal.DisplayItem.name != Items.COOKED_RAT_ID)
                return false;
        }

        return true;
    }

}

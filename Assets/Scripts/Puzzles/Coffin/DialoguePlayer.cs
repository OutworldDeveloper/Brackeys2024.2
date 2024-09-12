using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguePlayer : Pawn
{

    public event Action Updated;

    [SerializeField] private Prefab<Transform> _hud;

    public override bool ShowCursor => true;

    public override Transform CreateHud()
    {
        return _hud.Instantiate();
    }

    public void Setup(IDialogueProvider dialogueProvider)
    {

    }

    public void SelectOption()
    {

    }

}

public sealed class UI_DialogueHUD : MonoBehaviour
{

    private DialoguePlayer _player;

    public UI_DialogueHUD Setup(DialoguePlayer player)
    {
        _player = player;
        return this;
    }

}

public interface IDialogueProvider
{
    public IEnumerable<Question> GetQuestions(PlayerCharacter player);

}

public sealed class Question
{

    public readonly string Title;
    public readonly Sound Response;
    public readonly Action Action;

    public Question(string title, Sound response, Action action)
    {
        Title = title;
        Response = response;
        Action = action;
    }

}

public sealed class Coffin : MonoBehaviour, IDialogueProvider
{
    public bool HasInteractedWithPlayer { get; private set; }

    public IEnumerable<Question> GetQuestions(PlayerCharacter player)
    {
        if (HasInteractedWithPlayer == false)
        {
            yield return new Question("Who is there?", null, () => HasInteractedWithPlayer = true);
            yield break;
        }

        yield return new Question("No, the door doesn't open", null, () => { });
    }

}
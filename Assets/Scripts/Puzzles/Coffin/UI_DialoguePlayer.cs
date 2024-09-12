using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class UI_DialoguePlayer : UI_Panel
{

    [SerializeField] private RectTransform _parent;
    [SerializeField] private Prefab<UI_DialogueOption> _optionPrefab;

    private IDialogueTarget _target;

    private List<UI_DialogueOption> _currentOptions = new List<UI_DialogueOption>();

    public void Setup(IDialogueTarget target)
    {
        _target = target;
        SetVirtualCamera(target.VirtualCamera, CameraTransition.Move);
    }

    public override void OnReceivePlayerControl()
    {
        base.OnReceivePlayerControl();
        ShowNextOptions();
    }

    private void ShowNextOptions()
    {
        foreach (var option in _currentOptions)
        {
            Destroy(option.gameObject);
        }

        _currentOptions.Clear();

        foreach (var question in _target.GetQuestions(null))
        {
            var option = _optionPrefab.Instantiate();
            option.Setup(question);
            option.transform.SetParent(_parent, false);
            option.Selected += SelectOption;
            _currentOptions.Add(option);
        }
    }

    private void SelectOption(Question question)
    {
        Notification.Show(question.Title);
        ShowNextOptions();
    }

}

public interface IDialogueTarget
{
    public VirtualCamera VirtualCamera { get; }
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

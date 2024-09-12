using UnityEngine;
using System;
using TMPro;

public sealed class UI_DialogueOption : MonoBehaviour
{

    public event Action<Question> Selected;

    [SerializeField] private TextMeshProUGUI _label;

    private Question _question;

    public void Setup(Question question)
    {
        _question = question;
        _label.text = question.Title;
    }

    public void Select()
    {
        Selected?.Invoke(_question);
    }

}

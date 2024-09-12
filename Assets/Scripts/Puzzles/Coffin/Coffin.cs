using System.Collections.Generic;
using UnityEngine;

public sealed class Coffin : MonoBehaviour, IDialogueTarget
{
    [field: SerializeField] public VirtualCamera VirtualCamera { get; private set; }
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

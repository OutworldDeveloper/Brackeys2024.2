using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DeathScreen : UI_Panel
{

    public void OnLoadButtonPressed() { }

    public override bool CanUserClose()
    {
        return false;
    }

}

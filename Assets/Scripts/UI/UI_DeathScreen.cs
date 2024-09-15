using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_DeathScreen : UI_Panel
{
    public void OnLoadButtonPressed() { }

    public override bool CanRemoveAtWill()
    {
        return false;
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MenuButton()
    {
        Application.Quit();
    }

}

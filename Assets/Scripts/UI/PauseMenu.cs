using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ReturnToMenu(this);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

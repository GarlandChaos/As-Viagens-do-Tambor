using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.UI;

public class CreditsScreenController : APanelController
{
    //Inspector reference fields
    [SerializeField]
    GameEvent eventShowMainMenu = null;

    public void OnBackButton()
    {
        Hide();
        eventShowMainMenu.Raise();
    }
}

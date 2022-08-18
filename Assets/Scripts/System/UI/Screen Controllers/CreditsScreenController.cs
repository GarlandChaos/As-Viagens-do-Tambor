using UnityEngine;
using TamborGame.Events;

namespace TamborGame.UI 
{
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
}

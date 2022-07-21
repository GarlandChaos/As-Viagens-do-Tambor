using UnityEngine;
using TMPro;

namespace System.UI
{
    public class LoseScreenController : ADialogController
    {
        //Inspector reference fields
        [SerializeField]
        TMP_Text textLoseMessage = null;
        [SerializeField]
        GameEvent eventCloseLoseScreen = null;

        public void CloseLoseScreen()
        {
            eventCloseLoseScreen.Raise();
        }

        public void OnPlayerLose(string loserName)
        {
            textLoseMessage.text = "O jogador " + loserName + " perdeu por errar o palpite.";
        }
    }
}

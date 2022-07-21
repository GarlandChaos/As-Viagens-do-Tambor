using UnityEngine;
using TMPro;
using MLAPI;

namespace System.UI
{
    public class WinScreenController : ADialogController
    {
        //Inspector reference fields
        [SerializeField]
        TMP_Text textWinMessage = null;
        [SerializeField]
        CardTemplate[] cardTemplates = new CardTemplate[3];
        [SerializeField]
        RectTransform rtCardContainer = null;
        [SerializeField]
        GameEvent eventCloseWinScreen = null;

        public void ShowEnvelopeCards()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                cardTemplates[0].Setup(GameManager.instance._EnvelopePlaceCard);
                //cardTemplates[0].transform.SetParent(rtCardContainer, false);

                cardTemplates[1].Setup(GameManager.instance._EnvelopePersonCard);
                //cardTemplates[1].transform.SetParent(rtCardContainer, false);

                cardTemplates[2].Setup(GameManager.instance._EnvelopePracticeCard);
                //cardTemplates[2].transform.SetParent(rtCardContainer, false);
            }
        }

        public void CloseWinScreen()
        {
            eventCloseWinScreen.Raise();
        }

        public void OnGameOver(string victorName)
        {
            textWinMessage.text = "O jogador " + victorName + " acertou o palpite e venceu!";
            ShowEnvelopeCards();
        }
    }
}
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
        GameEvent eventCloseWinScreen = null, eventRequestEnvelopeCards = null;

        public void ShowEnvelopeCards()
        {
            if (NetworkManager.Singleton.IsServer)
                SetupCardTemplates(GameManager.instance._EnvelopePlaceCard, GameManager.instance._EnvelopePersonCard, GameManager.instance._EnvelopePracticeCard);
            else
                eventRequestEnvelopeCards.Raise();
        }

        public void ReceiveEnvelopeCardsFromServer(int placeCardId, int personCardId, int practiceCardId)
        {
            SetupCardTemplates(GameManager.instance.GetCardByID(placeCardId), GameManager.instance.GetCardByID(personCardId), GameManager.instance.GetCardByID(practiceCardId));
        }

        void SetupCardTemplates(Card placeCard, Card personCard, Card practiceCard)
        {
            cardTemplates[0].Setup(placeCard);
            cardTemplates[1].Setup(personCard);
            cardTemplates[2].Setup(practiceCard);
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
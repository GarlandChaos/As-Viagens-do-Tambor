using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace System.UI
{
    public class GuessScreenController : ADialogController
    {
        //Inspector reference fields
        [SerializeField]
        Button buttonSendGuess = null;
        [SerializeField]
        RectTransform rtAskIfWantToGuess = null, rtGuess = null, rtPlaceContainer = null, rtPeopleContainer = null, 
            rtPracticesContainer = null, rtWaiting = null, rtConfirmGuess = null, rtShowCardFromOtherPlayers = null, rtShowCardContainer = null;
        [SerializeField]
        GameEvent eventCloseGuessScreen = null, eventTryToWinWithGuess = null, eventRequestPersonCards = null, eventRequestPracticeCards = null;
        [SerializeField]
        TMP_Text textShowCard = null;
        [SerializeField]
        CardTemplate prefabCardTemplate = null;
        [SerializeField]
        CardTemplateGuess prefabCardTemplateGuess = null;
        
        //Runtime fields
        Card selectedPersonCard = null, selectedPracticeCard = null;

        private void OnEnable()
        {
            rtAskIfWantToGuess.gameObject.SetActive(true);
            rtGuess.gameObject.SetActive(false);
            rtWaiting.gameObject.SetActive(false);
            rtConfirmGuess.gameObject.SetActive(false);
            rtShowCardFromOtherPlayers.gameObject.SetActive(false);
        }

        public void CreateCardTemplate(Card c)
        {
            CardTemplateGuess ct = Instantiate(prefabCardTemplateGuess);
            ct.Setup(this, c);
            if (c.type == CardType.person)
                ct.transform.SetParent(rtPeopleContainer, false);
            else if (c.type == CardType.practice)
                ct.transform.SetParent(rtPracticesContainer, false);
        }

        public void OnYesButton()
        {
            CleanCardContainers();
            selectedPersonCard = null;
            selectedPracticeCard = null;
            buttonSendGuess.interactable = false;

            rtAskIfWantToGuess.gameObject.SetActive(false);
            rtGuess.gameObject.SetActive(true);

            CardTemplateGuess ctPlace = Instantiate(prefabCardTemplateGuess);
            ctPlace.Setup(this, GameManager.instance._CurrentPlaceGuess);
            ctPlace.transform.SetParent(rtPlaceContainer, false);

            List<Card> personCards = GameManager.instance.GetPersonCards();
            if(personCards != null)
                foreach (Card c in personCards)
                    CreateCardTemplate(c);
            else
                eventRequestPersonCards.Raise();

            List<Card> practiceCards = GameManager.instance.GetPracticeCards();
            if (practiceCards != null)
                foreach (Card c in practiceCards)
                    CreateCardTemplate(c);
            else
                eventRequestPracticeCards.Raise();
        }

        public void OnNoButton()
        {
            eventCloseGuessScreen.Raise();
        }

        public void OnConfirmButton()
        {
            rtGuess.gameObject.SetActive(false);
            rtWaiting.gameObject.SetActive(true);
            GameManager.instance.SetGuessCardsAndCheck(selectedPersonCard, selectedPracticeCard);
        }

        public void TryToWinWithGuess()
        {
            eventTryToWinWithGuess.Raise();
            eventCloseGuessScreen.Raise();
        }

        public void CleanCardContainers()
        {
            if (rtPlaceContainer.childCount > 0)
                Destroy(rtPlaceContainer.GetChild(0).gameObject);

            if (rtPeopleContainer.childCount > 0)
                for (int i = 0; i < rtPeopleContainer.childCount; i++)
                    Destroy(rtPeopleContainer.GetChild(i).gameObject);

            if (rtPracticesContainer.childCount > 0)
                for (int i = 0; i < rtPracticesContainer.childCount; i++)
                    Destroy(rtPracticesContainer.GetChild(i).gameObject);

            if (rtShowCardContainer.childCount > 0)
                Destroy(rtShowCardContainer.GetChild(0).gameObject);
        }

        public void SetSelectedPersonCard(Card person)
        {
            selectedPersonCard = person;
            if (selectedPracticeCard != null)
                buttonSendGuess.interactable = true;
        }

        public void SetSelectedPracticeCard(Card practice)
        {
            selectedPracticeCard = practice;
            if (selectedPersonCard != null)
                buttonSendGuess.interactable = true;
        }

        public void OnShowCardToPlayer(Card card, string playerName)
        {
            textShowCard.text = playerName.ToString() + " possui a seguinte carta:";
            CardTemplate ct = Instantiate(prefabCardTemplate);
            ct.Setup(card);
            ct.transform.SetParent(rtShowCardContainer, false);

            rtAskIfWantToGuess.gameObject.SetActive(false);
            rtGuess.gameObject.SetActive(false);
            rtWaiting.gameObject.SetActive(false);
            rtConfirmGuess.gameObject.SetActive(false);
            rtShowCardFromOtherPlayers.gameObject.SetActive(true);
        }

        public void OnAskForGuessConfirmation()
        {
            rtAskIfWantToGuess.gameObject.SetActive(false);
            rtGuess.gameObject.SetActive(false);
            rtWaiting.gameObject.SetActive(false);
            rtShowCardFromOtherPlayers.gameObject.SetActive(false);
            rtConfirmGuess.gameObject.SetActive(true);
        }
    }
}
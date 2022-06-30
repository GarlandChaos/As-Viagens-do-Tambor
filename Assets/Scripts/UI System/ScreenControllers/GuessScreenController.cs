﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace System.UI
{
    public class GuessScreenController : ADialogController
    {
        [SerializeField]
        Button buttonSendGuess;
        [SerializeField]
        RectTransform rtAskIfWantToGuess, rtGuess, rtPlaceContainer, rtPeopleContainer, rtPracticesContainer, rtWaiting, rtConfirmGuess, rtShowCardFromOtherPlayers, rtShowCardContainer;
        [SerializeField]
        GameEvent eventCloseGuessScreen, eventTryToWinWithGuess;
        [SerializeField]
        TMP_Text textShowCard;
        [SerializeField]
        CardTemplate prefabCardTemplate;
        [SerializeField]
        CardTemplateGuess prefabCardTemplateGuess;
        Card selectedPersonCard, selectedPracticeCard, selectedPlaceCard;
        bool confirmScreen = false;

        private void OnEnable()
        {
            if (!confirmScreen) //precisa mesmo disso?
            {
                rtAskIfWantToGuess.gameObject.SetActive(true);
                rtGuess.gameObject.SetActive(false);
                rtWaiting.gameObject.SetActive(false);
                rtConfirmGuess.gameObject.SetActive(false);
                rtShowCardFromOtherPlayers.gameObject.SetActive(false);
            }
            else
            {
                rtAskIfWantToGuess.gameObject.SetActive(false);
                rtGuess.gameObject.SetActive(false);
                rtWaiting.gameObject.SetActive(false);
                rtConfirmGuess.gameObject.SetActive(false);
                rtShowCardFromOtherPlayers.gameObject.SetActive(false);
            }
        }

        public void OnYesButton()
        {
            Debug.Log("Pressed yes button once?");
            CleanCardContainers();
            selectedPersonCard = null;
            selectedPracticeCard = null;
            buttonSendGuess.interactable = false;

            selectedPlaceCard = GameManager.instance._CurrentPlaceGuess;

            rtAskIfWantToGuess.gameObject.SetActive(false);
            rtGuess.gameObject.SetActive(true);

            CardTemplateGuess ctPlace = Instantiate(prefabCardTemplateGuess);
            ctPlace.Setup(this, selectedPlaceCard);
            ctPlace.transform.SetParent(rtPlaceContainer, false);

            foreach (Card c in GameManager.instance.GetPersonCards())
            {
                CardTemplateGuess ct = Instantiate(prefabCardTemplateGuess);
                ct.Setup(this, c);
                ct.transform.SetParent(rtPeopleContainer, false);
            }

            foreach (Card c in GameManager.instance.GetPracticeCards())
            {
                CardTemplateGuess ct = Instantiate(prefabCardTemplateGuess);
                ct.Setup(this, c);
                ct.transform.SetParent(rtPracticesContainer, false);
            }
        }

        public void OnNoButton()
        {
            eventCloseGuessScreen.Raise();
        }

        public void OnConfirmButton()
        {
            //envia pessoa, local e prática para o gamemanager...
            rtGuess.gameObject.SetActive(false);
            rtWaiting.gameObject.SetActive(true);
            GameManager.instance.SendGuessToOtherPlayers(selectedPlaceCard, selectedPersonCard, selectedPracticeCard);

        }

        public void TryToWinWithGuess()
        {
            eventTryToWinWithGuess.Raise();
        }

        public void CleanCardContainers()
        {
            if (rtPlaceContainer.childCount > 0)
                Destroy(rtPlaceContainer.GetChild(0).gameObject);

            if (rtPeopleContainer.childCount > 0)
            {
                for (int i = 0; i < rtPeopleContainer.childCount; i++)
                    Destroy(rtPeopleContainer.GetChild(i).gameObject);
            }

            if (rtPracticesContainer.childCount > 0)
            {
                for (int i = 0; i < rtPracticesContainer.childCount; i++)
                    Destroy(rtPracticesContainer.GetChild(i).gameObject);
            }

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

        public void OnShowCardToPlayer(Card card, int playerID)
        {
            textShowCard.text = "O jogador " + playerID.ToString() + " possui a seguinte carta:";
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
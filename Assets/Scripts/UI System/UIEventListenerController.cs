using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.UI
{
    public class UIEventListenerController : MonoBehaviour
    {
        private void Start()
        {
            UIManager.instance.RequestScreen("Room Manager Screen", true);
        }

        public void OnStartGame()
        {
            UIManager.instance.RequestScreen("Room Manager Screen", false);
            UIManager.instance.RequestScreen("Status Panel", true);
            UIManager.instance.RequestScreen("Notes", true);
        }

        public void OnDisplayDiceResults()
        {
            UIManager.instance.RequestScreen("Dices Results Panel", true);
        }

        public void OnEndOfMove()
        {
            UIManager.instance.RequestScreen("Dices Results Panel", false);
        }

        public void OnShowMainMenu()
        {
            UIManager.instance.RequestScreen("Room Manager Screen", true);
        }

        public void OnShowCredits()
        {
            UIManager.instance.RequestScreen("Credits Screen", true);
        }

        public void OnOpenExtraCardScreen()
        {
            UIManager.instance.RequestScreen("Extra Card Screen", true);
        }

        public void CloseExtraCardScreen()
        {
            UIManager.instance.RequestScreen("Extra Card Screen", false);
        }

        public void OnOpenAskIfWantToGoToPlaceScreen()
        {
            CloseExtraCardScreen();
            UIManager.instance.RequestScreen("AskIfWantToGoToPlace Screen", true);
        }

        public void OnWin()
        {
            UIManager.instance.RequestScreen("Win Screen", true);
        }

        public void OnLose()
        {
            UIManager.instance.RequestScreen("Lose Screen", true);
        }

        public void OnCloseWinScreen()
        {
            UIManager.instance.RequestScreen("Win Screen", false);
        }

        public void OnCloseLoseScreen()
        {
            UIManager.instance.RequestScreen("Lose Screen", false);
        }

        public void OnCloseAskIfWantToGoToPlaceScreen()
        {
            UIManager.instance.RequestScreen("AskIfWantToGoToPlace Screen", false);
        }

        public void OnOpenGuessScreen()
        {
            UIManager.instance.RequestScreen("Guess Screen", true);
        }

        public void OnCloseGuessScreen()
        {
            UIManager.instance.RequestScreen("Guess Screen", false);
        }
    }
}

using UnityEngine;

namespace System.UI
{
    public class WinScreenController : ADialogController
    {
        //Inspector reference fields
        [SerializeField]
        CardTemplate prefabCardTemplate = null;
        [SerializeField]
        RectTransform rtCardContainer = null;
        [SerializeField]
        GameEvent eventCloseWinScreen = null;

        public void OnEnable()
        {
            if (GameManager.instance._CurrentPracticeGuess != null)
                ShowEnvelopeCards();
        }

        public void ShowEnvelopeCards()
        {
            CardTemplate cPlace = Instantiate(prefabCardTemplate);
            cPlace.Setup(GameManager.instance._EnvelopePlaceCard);
            cPlace.transform.SetParent(rtCardContainer, false);

            CardTemplate cPerson = Instantiate(prefabCardTemplate);
            cPerson.Setup(GameManager.instance._EnvelopePersonCard);
            cPerson.transform.SetParent(rtCardContainer, false);

            CardTemplate cPractice = Instantiate(prefabCardTemplate);
            cPractice.Setup(GameManager.instance._EnvelopePracticeCard);
            cPractice.transform.SetParent(rtCardContainer, false);
        }

        public void CloseWinScreen()
        {
            eventCloseWinScreen.Raise();
        }
    }
}
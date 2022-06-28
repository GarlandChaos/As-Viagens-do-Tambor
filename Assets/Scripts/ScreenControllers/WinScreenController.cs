using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreenController : ADialogController
{
    [SerializeField]
    CardTemplate prefabCardTemplate;
    [SerializeField]
    RectTransform rtCardContainer;
    [SerializeField]
    GameEvent eventCloseWinScreen;

    public void OnEnable()
    {
        if(GameManager.instance._CurrentPracticeGuess != null)
        {
            ShowEnvelopeCards();
        }
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

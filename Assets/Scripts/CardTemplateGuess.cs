using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardTemplateGuess : CardTemplate
{
    GuessScreenController guessScreen;

    public void CardSelect()
    {
        if(card.type != CardType.place)
        {
            Debug.Log("Clicou em : " + card.name);
            if (card.type == CardType.person)
            {
                guessScreen.SetSelectedPersonCard(card);
            }
            else if (card.type == CardType.practice)
            {
                guessScreen.SetSelectedPracticeCard(card);
            }
        }
        else
        {
            Debug.Log("Clicou em : " + card.name + " porém já está definido o card de lugar.");
        }

    }

    public void Setup(GuessScreenController g, Card c)
    {
        base.Setup(c);
        guessScreen = g;
    }
}

using TamborGame.Gameplay;

namespace TamborGame.UI
{
    public class CardTemplateGuess : CardTemplate
    {
        GuessScreenController guessScreen = null;

        public void CardSelect()
        {
            if (card.type != CardType.place)
            {
                if (card.type == CardType.person)
                    guessScreen.SetSelectedPersonCard(card);
                else if (card.type == CardType.practice)
                    guessScreen.SetSelectedPracticeCard(card);
            }
        }

        public void Setup(GuessScreenController g, Card c)
        {
            base.Setup(c);
            guessScreen = g;
        }
    }
}
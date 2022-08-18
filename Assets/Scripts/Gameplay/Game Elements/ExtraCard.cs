using UnityEngine;

namespace TamborGame.Gameplay
{
    public enum Effect
    {
        reroll, //jogar dados novamente
        loseTurn, //uma rodada sem jogar
        goToPlace, //vai para o lugar e d� palpite (opcional) e encerra o turno
        goToPlaceOptional, //jogador decide se pe�o vai para determinado lugar para dar palpite
        goToPublicMarketAndChoosePlace, //pe�o vai para o Mercado P�blico e decide para qual lugar quer ir entre todos
        returnToPreviousPlaceAndGuess //retornar para o local que estava anteriormente e d� palpite de l�

        //revisar cards 9, 13 e 15
        //trazer o pe�o da Mestra Gri� Elaine antes de dar o palpite
        //todos participantes devem ir para o Parque da Reden��o e dar palpite de l�
        //recitar poema
    }

    [CreateAssetMenu]
    public class ExtraCard : Card
    {
        public string description = null;
        public Effect effect = Effect.reroll;
        public PlaceName placeToGo = PlaceName.None;
        public bool askIfWantToGuess = false;

        ExtraCard()
        {
            type = CardType.extra;
        }
    }
}
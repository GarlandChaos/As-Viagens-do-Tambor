using UnityEngine;

namespace TamborGame.Gameplay
{
    public enum Effect
    {
        reroll, //jogar dados novamente
        loseTurn, //uma rodada sem jogar
        goToPlace, //vai para o lugar e dá palpite (opcional) e encerra o turno
        goToPlaceOptional, //jogador decide se peão vai para determinado lugar para dar palpite
        goToPublicMarketAndChoosePlace, //peão vai para o Mercado Público e decide para qual lugar quer ir entre todos
        returnToPreviousPlaceAndGuess //retornar para o local que estava anteriormente e dá palpite de lá

        //revisar cards 9, 13 e 15
        //trazer o peão da Mestra Griô Elaine antes de dar o palpite
        //todos participantes devem ir para o Parque da Redenção e dar palpite de lá
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
using UnityEngine;

namespace TamborGame.Gameplay 
{
    public enum PlaceName
    {
        None,
        PracaAlfandega,
        ColoniaAfricana,
        MariaConceicao,
        IgrejaRosario,
        Ilhota,
        MercadoPublico,
        PracaTambor,
        QuilomboAreal,
        ParqueRedencao,
        Restinga,
        RubemBerta
    }

    public class Place : MonoBehaviour
    {
        public Card cardPlace = null;
        public PlaceName placeName = PlaceName.None;
    }
}
using UnityEngine;

namespace TamborGame.Gameplay
{
    public enum CardType
    {
        person,
        place,
        practice,
        extra
    }

    [CreateAssetMenu]
    public class Card : ScriptableObject
    {
        public int id = -1;
        public CardType type = CardType.place;
        public Sprite sprite = null;
    }
}
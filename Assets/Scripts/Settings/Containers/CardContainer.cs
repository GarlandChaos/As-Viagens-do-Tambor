using System.Collections.Generic;
using UnityEngine;
using TamborGame.Gameplay;

namespace TamborGame.Settings
{
    [CreateAssetMenu]
    public class CardContainer : ScriptableObject
    {
        [SerializeField]
        List<Card> cards = new List<Card>();

        public List<Card> _Cards { get => cards; set { cards = value; } }
    }
}
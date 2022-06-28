using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CardContainer : ScriptableObject
{
    [SerializeField]
    List<Card> cards = new List<Card>();

    public List<Card> _Cards { get => cards; set { cards = value; } }
}

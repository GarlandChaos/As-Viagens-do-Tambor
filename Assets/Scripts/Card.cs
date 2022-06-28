using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public int id;
    //public string name;
    public CardType type;
    public Sprite sprite;
}

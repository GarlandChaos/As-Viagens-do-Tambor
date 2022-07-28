using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardTemplate : MonoBehaviour
{
    [SerializeField]
    protected Image imageCard = null;
    protected Card card = null;

    public void Setup(Card c)
    {
        card = c;
        imageCard.sprite = c.sprite;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class NoteCard : MonoBehaviour
{
    TMP_Text textCard;
    [SerializeField]
    Card card;

    private void Awake()
    {
        textCard = GetComponent<TMP_Text>();    
    }

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public void CardCheck(Card c)
    {
        if(c == card)
        {
            textCard.fontStyle = FontStyles.Strikethrough;
        }
    }
}

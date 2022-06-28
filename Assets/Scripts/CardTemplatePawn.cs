using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardTemplatePawn : MonoBehaviour
{
    RoomManagerScreen roomManagerScreen;
    [SerializeField]
    Image pawnImage;
    Pawn pawn;

    public void CardSelect()
    {
        roomManagerScreen.SetSelectedPawn(pawn);
    }

    public void Setup(RoomManagerScreen r, Pawn p)
    {
        pawn = p;
        pawnImage.sprite = p.spritePawn;
        roomManagerScreen = r;
    }
}

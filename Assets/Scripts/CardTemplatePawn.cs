using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.UI;

public class CardTemplatePawn : MonoBehaviour
{
    //Inspector reference fields
    [SerializeField]
    Image pawnImage;
    [SerializeField]
    Pawn pawn;
    [SerializeField]
    RoomManagerScreen roomManagerScreen;

    //Properties
    public string _PawnName { get => pawn.name; }

    public void CardSelect()
    {
        roomManagerScreen.SetSelectedPawn(pawn);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (pawn != null)
            pawnImage.sprite = pawn.spritePawn;
    }
#endif
}

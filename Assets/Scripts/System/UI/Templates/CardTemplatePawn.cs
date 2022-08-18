using UnityEngine;
using UnityEngine.UI;
using TamborGame.Gameplay;

namespace TamborGame.UI
{
    public class CardTemplatePawn : MonoBehaviour
    {
        //Inspector reference fields
        [SerializeField]
        Image pawnImage = null;
        [SerializeField]
        Pawn pawn = null;
        [SerializeField]
        RoomManagerScreen roomManagerScreen = null;

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
}
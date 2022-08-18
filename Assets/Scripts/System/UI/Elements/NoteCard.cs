using UnityEngine;
using TMPro;
using TamborGame.Gameplay;

namespace TamborGame.UI 
{
    [RequireComponent(typeof(TMP_Text))]
    public class NoteCard : MonoBehaviour
    {
        //Inspector reference fields
        [SerializeField]
        Card card = null;
        TMP_Text textCard = null;

        private void Awake()
        {
            textCard = GetComponent<TMP_Text>();
        }

        public void CardCheck(Card c)
        {
            if (c == card)
                textCard.fontStyle = FontStyles.Strikethrough;
        }
    }
}
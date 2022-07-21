using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace System.UI
{
    public class AskIfWantToGoToPlaceController : ADialogController
    {
        //Inspector reference fields
        [SerializeField]
        TMP_Text textAskPlayer = null;
        ExtraCard currentExtraCard = null;
        [SerializeField]
        GameEvent eventSendExtraCardToPlayerFromOptionalChoice = null, eventCloseAskIfWantToGoToPlaceScreen = null, eventRequestChangeOfPlayerTurnToPlayer = null;

        public void ReceiveExtraCard(ExtraCard extraCard)
        {
            currentExtraCard = extraCard;
        }

        public void YesButton()
        {
            eventSendExtraCardToPlayerFromOptionalChoice.Raise(currentExtraCard);
            eventCloseAskIfWantToGoToPlaceScreen.Raise();
        }

        public void NoButton()
        {
            eventCloseAskIfWantToGoToPlaceScreen.Raise();
            eventRequestChangeOfPlayerTurnToPlayer.Raise();
        }
    }
}

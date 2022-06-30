using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace System.UI
{
    public class AskIfWantToGoToPlaceController : ADialogController
    {
        [SerializeField]
        TMP_Text textAskPlayer;
        ExtraCard currentExtraCard;
        [SerializeField]
        GameEvent eventSendExtraCardToPlayerFromOptionalChoice, eventCloseAskIfWantToGoToPlaceScreen, eventRequestChangeOfPlayerTurnToPlayer;

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

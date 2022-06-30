using UnityEngine;
using TMPro;

namespace System.UI
{
    public class StatusController : APanelController
    {
        [SerializeField]
        TMP_Text textStatus;

        public void OnUpdateStatusPanel(bool isOwnerTurn)
        {
            if (isOwnerTurn)
                textStatus.text = "Sua vez! Aperte R para rolar os dados.";
            else
                textStatus.text = "Aguardando o outro jogador...";
        }
    }
}
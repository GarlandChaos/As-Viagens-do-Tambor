using UnityEngine;
using TMPro;

namespace System.UI
{
    public class StatusController : APanelController
    {
        //Inspector reference fields
        [SerializeField]
        TMP_Text textStatus = null;

        private void Awake()
        {
            textStatus.text = "Aguardando o outro jogador...";
        }

        public void OnUpdateStatusPanel(bool isOwnerTurn)
        {
            if (isOwnerTurn)
                textStatus.text = "Sua vez! Role os dados.";
            else
                textStatus.text = "Aguardando o outro jogador...";
        }
    }
}
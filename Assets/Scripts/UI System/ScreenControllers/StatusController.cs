using UnityEngine;
using TMPro;
using MLAPI;

namespace System.UI
{
    public class StatusController : APanelController
    {
        //Inspector reference fields
        [SerializeField]
        TMP_Text textStatus = null;

        private void OnEnable()
        {
            if(NetworkManager.Singleton != null)
                OnUpdateStatusPanel(NetworkManager.Singleton.IsServer);
        }

        public void OnUpdateStatusPanel(bool isOwnerTurn)
        {
            if (isOwnerTurn)
                textStatus.text = "Sua vez! Role os dados.";
            else
                textStatus.text = "Aguardando o outro jogador...";
        }

        public void OnGameOverScreen()
        {
            textStatus.text = "Fim de jogo!";
        }
    }
}
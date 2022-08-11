using UnityEngine;
using TMPro;
using MLAPI;
using UnityEngine.UI;

namespace System.UI
{
    public class StatusController : APanelController
    {
        //Inspector reference fields
        [SerializeField]
        TMP_Text textStatus = null;
        [SerializeField]
        RectTransform statusBar = null, background = null;

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

            RefreshPanel();
        }

        public void OnGameOverScreen()
        {
            textStatus.text = "Fim de jogo!";
            RefreshPanel();
        }

        void RefreshPanel()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(statusBar);
            LayoutRebuilder.ForceRebuildLayoutImmediate(background);
        }
    }
}
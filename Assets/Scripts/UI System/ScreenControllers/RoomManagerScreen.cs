using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Transports.UNET;
using TMPro;

namespace System.UI
{
    public class RoomManagerScreen : APanelController
    {
        [SerializeField]
        RectTransform createOrSearchRoomDialog, createRoomDialog, waitingForPlayersDialog, selectRoomDialog, enterRoomDialog,
                      createRoomPawnContainer, enterRoomPawnContainer;
        [SerializeField]
        TMP_InputField playerNameCreateInputField, roomNameInputField, playerNameSearchInputField;
        [SerializeField]
        Button createRoomPlayButton, enterRoomPlayButton;
        [SerializeField]
        CardTemplatePawn prefabCardTemplatePawn;
        [SerializeField]
        GameObject roomTemplatePrefab;
        Pawn selectedPawn;
        List<string> adressesFoundList;
        bool connectedToHost = false;
        [SerializeField]
        GameEvent eventRequestPawns, eventCreatePlayerPawn;
        [SerializeField]
        PawnContainer pawnsContainer = null;

        private void Awake()
        {
            MyNetworkDiscovery.instance.Initialize();

            if (playerNameCreateInputField != null)
                playerNameCreateInputField.onValueChanged.AddListener(delegate { CreateRoomInputChange(); });

            if (roomNameInputField != null)
                roomNameInputField.onValueChanged.AddListener(delegate { CreateRoomInputChange(); });

            if (playerNameSearchInputField != null)
                playerNameSearchInputField.onValueChanged.AddListener(delegate { EnterRoomInputChange(); });

            if (createRoomPlayButton != null)
                createRoomPlayButton.interactable = false;

            if (enterRoomPlayButton != null)
                enterRoomPlayButton.interactable = false;

            adressesFoundList = new List<string>();
        }

        private void OnEnable()
        {
            MyNetworkDiscovery.instance.Initialize();
            createOrSearchRoomDialog.gameObject.SetActive(true);
            createRoomDialog.gameObject.SetActive(false);
            waitingForPlayersDialog.gameObject.SetActive(false);
            selectRoomDialog.gameObject.SetActive(false);
            enterRoomDialog.gameObject.SetActive(false);
            selectedPawn = null;
        }

        private void OnDisable()
        {
            if (MyNetworkDiscovery.instance.running)
                MyNetworkDiscovery.instance.StopBroadcast();
        }

        public void SetSelectedPawn(Pawn pawn)
        {
            selectedPawn = pawn;
            GameManager.instance.clientPawn = pawn;

            if (createRoomDialog.gameObject.activeSelf)
                CreateRoomInputChange();
            else if (enterRoomDialog.gameObject.activeSelf)
                EnterRoomInputChange();
        }

        public void FillRoomsTemplates()
        {
            if (MyNetworkDiscovery.instance.addresses.Count > 0 && !connectedToHost)
            {
                int i = 0;
                foreach (KeyValuePair<string, string> kvp in MyNetworkDiscovery.instance.addresses)
                {
                    if (!adressesFoundList.Contains(kvp.Key))
                    {
                        adressesFoundList.Add(kvp.Key);
                        GameObject go = Instantiate(roomTemplatePrefab);
                        RoomTemplate roomTemplate = go.GetComponent<RoomTemplate>();
                        RectTransform goRT = go.GetComponent<RectTransform>();
                        if (roomTemplate != null)
                        {
                            float sizeIncrement = i * goRT.rect.size.y;
                            float margin = i * 10f;
                            if (i > 0)
                                selectRoomDialog.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, selectRoomDialog.sizeDelta.y + goRT.rect.size.y + 10f);

                            go.transform.SetParent(selectRoomDialog, false);
                            goRT.anchoredPosition = new Vector2(
                                goRT.anchoredPosition.x,
                                goRT.anchoredPosition.y - sizeIncrement - margin);
                            roomTemplate.SetRoomName(kvp.Value);
                            roomTemplate.GetPlayButton().interactable = false;
                            roomTemplate.GetPlayButton().onClick.AddListener(
                            delegate
                            {
                                NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = kvp.Key;
                                EnterRoom();
                                connectedToHost = true;
                            });
                            roomTemplate.GetPlayButton().interactable = true;
                        }
                    }
                    i++;
                }
            }
        }

        public void GoToCreateRoomScreen()
        {
            createOrSearchRoomDialog.gameObject.SetActive(false);
            createRoomDialog.gameObject.SetActive(true);
            foreach (Pawn p in pawnsContainer._Pawns)
            {
                CardTemplatePawn ct = Instantiate(prefabCardTemplatePawn);
                ct.Setup(this, p);
                ct.transform.SetParent(createRoomPawnContainer, false);
            }
        }

        public void GoToSelectRoomScreen()
        {
            createOrSearchRoomDialog.gameObject.SetActive(false);
            selectRoomDialog.gameObject.SetActive(true);
            MyNetworkDiscovery.instance.StartAsClient();
        }

        public void CreateRoomAndPlay()
        {
            createRoomDialog.gameObject.SetActive(false);
            waitingForPlayersDialog.gameObject.SetActive(true);
            MyNetworkDiscovery.instance.broadcastData = roomNameInputField.text;
            MyNetworkDiscovery.instance.StartAsServer();
            NetworkManager.Singleton.StartHost();
            eventCreatePlayerPawn.Raise();
        }

        public void EnterRoom()
        {
            selectRoomDialog.gameObject.SetActive(false);
            //send an RPC
            enterRoomDialog.gameObject.SetActive(true);
            NetworkManager.Singleton.StartClient();
            StartCoroutine(RequestPawnsCoroutine());
        }

        public void ClientPlay()
        {
            eventCreatePlayerPawn.Raise();
            Hide();
        }

        void CreateRoomInputChange()
        {
            if (!createRoomPlayButton.interactable && playerNameCreateInputField.text.Length > 0 && roomNameInputField.text.Length > 0 && selectedPawn != null)
            {
                createRoomPlayButton.interactable = true;
            }
            else if (
                (createRoomPlayButton.interactable && playerNameCreateInputField.text.Length == 0) ||
                (createRoomPlayButton.interactable && roomNameInputField.text.Length == 0))
            {
                createRoomPlayButton.interactable = false;
            }
        }

        void EnterRoomInputChange()
        {
            if (!enterRoomPlayButton.interactable && playerNameSearchInputField.text.Length > 0 && selectedPawn != null)
                enterRoomPlayButton.interactable = true;
            else if (enterRoomPlayButton.interactable && playerNameSearchInputField.text.Length == 0)
                enterRoomPlayButton.interactable = false;
        }

        public void OnSendServerPawnToRoomManager(string serverPawn)
        {
            foreach (Pawn p in pawnsContainer._Pawns) //menos o peão do host!
            {
                if (p.name != serverPawn)
                {
                    CardTemplatePawn ct = Instantiate(prefabCardTemplatePawn);
                    ct.Setup(this, p);
                    ct.transform.SetParent(enterRoomPawnContainer, false);
                }
            }
        }

        public void OnCreditsScreen()
        {
            //show credits screen
        }

        public void OnQuitGame()
        {
            Application.Quit();
        }

        IEnumerator RequestPawnsCoroutine()
        {
            if (NetworkManager.Singleton.LocalClientId == 0)
            {
                yield return new WaitForEndOfFrame();
                StartCoroutine(RequestPawnsCoroutine());
            }
            else
            {
                yield return new WaitForEndOfFrame();
                eventRequestPawns.Raise();
            }
        }
    }
}

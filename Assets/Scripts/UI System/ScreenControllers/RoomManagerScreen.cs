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
        //Inspector reference fields
        [SerializeField]
        RectTransform mainMenuDialog = null, createRoomDialog = null, waitingForPlayersDialog = null, selectRoomDialog = null, selectRoomDialogBG = null, enterRoomDialog = null,
                      createRoomPawnContainer = null, enterRoomPawnContainer = null;
        [SerializeField]
        TMP_InputField playerNameCreateInputField = null, roomNameInputField = null, playerNameSearchInputField = null;
        [SerializeField]
        Button createRoomPlayButton = null, backToMainMenuButton = null, enterRoomPlayButton = null, playGameButton = null;
        [SerializeField]
        TMP_Text numberOfPlayersConnectedText = null;
        [SerializeField]
        List<CardTemplatePawn> enterRoomCardTemplatePawnList = new List<CardTemplatePawn>();
        [SerializeField]
        GameObject roomTemplatePrefab = null;
        [SerializeField]
        GameEvent eventRequestPawns, eventCreatePlayerPawn, eventShowCredits, eventReadyToPlay;
        [SerializeField]
        InterpolationSettings animationSettings = null;

        Pawn selectedPawn = null;
        List<string> adressesFoundList = new List<string>();
        bool connectedToHost = false;

        private void Awake()
        {
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

            if (playGameButton != null)
                playGameButton.interactable = false;
        }

        private void OnEnable()
        {
            mainMenuDialog.gameObject.SetActive(true);
            createRoomDialog.gameObject.SetActive(false);
            waitingForPlayersDialog.gameObject.SetActive(false);
            selectRoomDialog.gameObject.SetActive(false);
            enterRoomDialog.gameObject.SetActive(false);
            selectedPawn = null;
            
            foreach (CardTemplatePawn ctp in enterRoomCardTemplatePawnList)
                ctp.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if(NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;

            if (MyNetworkDiscovery.instance.running)
                MyNetworkDiscovery.instance.StopBroadcast();
        }

        private void Singleton_OnClientConnectedCallback(ulong obj)
        {
            int playersCount = NetworkManager.Singleton.ConnectedClients.Count;
            numberOfPlayersConnectedText.text = playersCount.ToString() + "/4";
            
            if (playersCount > 1 && playersCount < 5)
                playGameButton.interactable = true;
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
                            go.transform.SetParent(selectRoomDialogBG, false);
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
                backToMainMenuButton.transform.SetAsLastSibling();
            }
        }

        public async void OnBackToMainMenuScreen()
        {
            if (MyNetworkDiscovery.instance.running)
                MyNetworkDiscovery.instance.StopBroadcast();

            await CloseScreenAnimation(selectRoomDialog);
            await OpenScreenAnimation(mainMenuDialog);
        }

        public async void GoToCreateRoomScreen()
        {
            MyNetworkDiscovery.instance.Initialize();
            mainMenuDialog.gameObject.SetActive(false);
            await OpenScreenAnimation(createRoomDialog);
        }

        public async void GoToSelectRoomScreen()
        {
            MyNetworkDiscovery.instance.Initialize();
            mainMenuDialog.gameObject.SetActive(false);
            await OpenScreenAnimation(selectRoomDialog);
            MyNetworkDiscovery.instance.StartAsClient();
        }

        public async void CreateRoom()
        {
            await CloseScreenAnimation(createRoomDialog);
            await OpenScreenAnimation(waitingForPlayersDialog);
            MyNetworkDiscovery.instance.broadcastData = roomNameInputField.text;
            MyNetworkDiscovery.instance.StartAsServer();
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
            eventCreatePlayerPawn.Raise(NetworkManager.Singleton.LocalClientId, selectedPawn.name);
        }

        public async void EnterRoom()
        {
            await CloseScreenAnimation(selectRoomDialog);
            await OpenScreenAnimation(enterRoomDialog);
            NetworkManager.Singleton.StartClient();
            StartCoroutine(RequestPawnsCoroutine());
        }

        public void HostPlay()
        {
            eventReadyToPlay.Raise();
            Hide();
        }

        public void ClientPlay()
        {
            eventCreatePlayerPawn.Raise(NetworkManager.Singleton.LocalClientId, selectedPawn.name);
            Hide();
        }

        void CreateRoomInputChange()
        {
            if (!createRoomPlayButton.interactable && playerNameCreateInputField.text.Length > 0 && roomNameInputField.text.Length > 0 && selectedPawn != null)
                createRoomPlayButton.interactable = true;
            else if (
                (createRoomPlayButton.interactable && playerNameCreateInputField.text.Length == 0) ||
                (createRoomPlayButton.interactable && roomNameInputField.text.Length == 0))
                createRoomPlayButton.interactable = false;
        }

        void EnterRoomInputChange()
        {
            if (!enterRoomPlayButton.interactable && playerNameSearchInputField.text.Length > 0 && selectedPawn != null)
                enterRoomPlayButton.interactable = true;
            else if (enterRoomPlayButton.interactable && playerNameSearchInputField.text.Length == 0)
                enterRoomPlayButton.interactable = false;
        }

        public void OnSendAvailablePawnsToRoomManager(string[] pawnNames)
        {
            foreach (CardTemplatePawn ctp in enterRoomCardTemplatePawnList)
            {
                bool uniqueName = true;
                foreach(string s in pawnNames)
                {
                    if (ctp._PawnName == s)
                        uniqueName = false;
                }

                if (!uniqueName)
                    ctp.gameObject.SetActive(false);
            }
        }

        public void OnCreditsScreen()
        {
            Hide();
            eventShowCredits.Raise();
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

        IEnumerator OpenScreenAnimation(RectTransform screen)
        {
            screen.gameObject.SetActive(true);
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            float timer = 0f;
            Vector3 startScale = screen.localScale;
            startScale.y = 0;
            Vector3 endScale = Vector3.one;

            while (timer < 1f)
            {
                timer += Time.deltaTime / animationSettings._Duration;
                screen.localScale = Vector3.Lerp(startScale, endScale, animationSettings._Curve.Evaluate(timer));
                yield return wait;
            }
        }

        IEnumerator CloseScreenAnimation(RectTransform screen)
        {
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            float timer = 0f;
            Vector3 startScale = Vector3.one;
            Vector3 endScale = screen.localScale;
            endScale.y = 0;

            while (timer < 1f)
            {
                timer += Time.deltaTime / animationSettings._Duration;
                screen.localScale = Vector3.Lerp(startScale, endScale, animationSettings._Curve.Evaluate(timer));
                yield return wait;
            }

            screen.gameObject.SetActive(false);
        }
    }
}

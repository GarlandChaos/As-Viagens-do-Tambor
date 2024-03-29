using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Transports.UNET;
using TMPro;
using TamborGame.Gameplay;
using TamborGame.Events;
using TamborGame.Utilities;
using TamborGame.Settings;

namespace TamborGame.UI
{
    public class RoomManagerScreen : APanelController
    {
        //Inspector reference fields
        [SerializeField]
        RectTransform mainMenuDialog = null, createRoomDialog = null, waitingForPlayersDialog = null, selectRoomDialog = null, 
            selectRoomDialogBG = null, enterRoomDialog = null, waitingForHostToStartDialog = null, enterRoomPawnContainer = null;
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
        GameEvent eventRequestPawns, eventCreatePlayerPawn = null, eventShowCredits = null, eventReadyToPlay = null, eventBackToMainMenu = null, eventSetPlayerName = null;
        [SerializeField]
        InterpolationSettings animationSettings = null;

        //Runtime fields
        Pawn selectedPawn = null;
        int playersConnected = 0;
        int minPlayers = 2;
        int maxPlayers = 2;

        private void Awake()
        {
            if (playerNameCreateInputField != null)
            {
                playerNameCreateInputField.onValueChanged.AddListener(delegate { CreateRoomInputChange(); });
                playerNameCreateInputField.onSubmit.AddListener(delegate { OnSubmitPlayerNameCreateInputField(); });
            }

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

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
        }

        private void OnDestroy()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        }

        private void OnEnable()
        {
            mainMenuDialog.gameObject.SetActive(true);
            createRoomDialog.gameObject.SetActive(false);
            waitingForPlayersDialog.gameObject.SetActive(false);
            selectRoomDialog.gameObject.SetActive(false);
            enterRoomDialog.gameObject.SetActive(false);
            waitingForHostToStartDialog.gameObject.SetActive(false);
            selectedPawn = null;
            playersConnected = 0;
            
            foreach (CardTemplatePawn ctp in enterRoomCardTemplatePawnList)
                ctp.gameObject.SetActive(true);
        }

        void Singleton_OnClientConnectedCallback(ulong obj)
        {
            if (NetworkManager.Singleton.IsServer && NetworkManager.Singleton.LocalClientId == obj)
            {
                playersConnected++;
                UpdateWaitingForPlayersDialogInfo();
            }
        }

        void Singleton_OnClientDisconnectCallback(ulong obj)
        {
            playersConnected--;
            UpdateWaitingForPlayersDialogInfo();
        }

        void OnSubmitPlayerNameCreateInputField()
        {
            if (playerNameCreateInputField.isFocused)
            {
                playerNameCreateInputField.DeactivateInputField();
                roomNameInputField.Select();
            }
        }

        void UpdateWaitingForPlayersDialogInfo()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                numberOfPlayersConnectedText.text = playersConnected.ToString() + "/" + maxPlayers.ToString();

                if (playersConnected >= minPlayers && playersConnected <= maxPlayers)
                    playGameButton.interactable = true;
                else
                    playGameButton.interactable = false;
            }
        }

        public void SetSelectedPawn(Pawn pawn)
        {
            selectedPawn = pawn;

            if (createRoomDialog.gameObject.activeSelf)
                CreateRoomInputChange();
            else if (enterRoomDialog.gameObject.activeSelf)
                EnterRoomInputChange();
        }

        public void FillRoomsTemplates()
        {
            //if (GameManager.instance.DEBUGROOM)
            //{
            //    GameObject go = Instantiate(roomTemplatePrefab);
            //    RoomTemplate roomTemplate = go.GetComponent<RoomTemplate>();

            //    if (roomTemplate != null)
            //    {
            //        go.transform.SetParent(selectRoomDialogBG, false);
            //        roomTemplate.SetRoomName("127.0.0.1");
            //        roomTemplate.GetPlayButton().interactable = false;
            //        roomTemplate.GetPlayButton().onClick.AddListener(
            //        delegate
            //        {
            //            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = "127.0.0.1";
            //            EnterRoom();
            //        });
            //        roomTemplate.GetPlayButton().interactable = true;
            //    }
            //    backToMainMenuButton.transform.SetAsLastSibling();
            //}

            if (CustomNetworkDiscovery.instance.addresses.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in CustomNetworkDiscovery.instance.addresses)
                {
                    GameObject go = Instantiate(roomTemplatePrefab);
                    RoomTemplate roomTemplate = go.GetComponent<RoomTemplate>();

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
                        });
                        roomTemplate.GetPlayButton().interactable = true;
                    }
                }
                backToMainMenuButton.transform.SetAsLastSibling();
            }
        }

        public async void OnBackToMainMenuScreen(RectTransform currentScreen)
        {
            foreach (RoomTemplate rt in selectRoomDialogBG.GetComponentsInChildren<RoomTemplate>())
                Destroy(rt.gameObject);

            CustomNetworkDiscovery.instance.addresses.Clear();
            CustomNetworkDiscovery.instance.StopBroadcast();

            eventBackToMainMenu.Raise();
            await CloseScreenAnimation(currentScreen);
            await OpenScreenAnimation(mainMenuDialog);
        }

        public async void GoToCreateRoomScreen()
        {
            mainMenuDialog.gameObject.SetActive(false);
            await OpenScreenAnimation(createRoomDialog);
        }

        public async void GoToSelectRoomScreen()
        {
            mainMenuDialog.gameObject.SetActive(false);
            await OpenScreenAnimation(selectRoomDialog);
            FillRoomsTemplates();

            CustomNetworkDiscovery.instance.Initialize();    
            CustomNetworkDiscovery.instance.StartAsClient();
        }

        public async void CreateRoom()
        {
            await CloseScreenAnimation(createRoomDialog);
            await OpenScreenAnimation(waitingForPlayersDialog);

            CustomNetworkDiscovery.instance.Initialize();
            CustomNetworkDiscovery.instance.broadcastData = roomNameInputField.text;
            CustomNetworkDiscovery.instance.StartAsServer();
            
            NetworkManager.Singleton.StartHost();

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
            eventSetPlayerName.Raise(NetworkManager.Singleton.LocalClientId, playerNameCreateInputField.text);
            eventReadyToPlay.Raise();
            CustomNetworkDiscovery.instance.addresses.Clear();
            CustomNetworkDiscovery.instance.StopBroadcast();
            Hide();
        }

        public async void ClientPlay()
        {
            await CloseScreenAnimation(enterRoomDialog);
            await OpenScreenAnimation(waitingForHostToStartDialog);
            eventCreatePlayerPawn.Raise(NetworkManager.Singleton.LocalClientId, selectedPawn.name);
            eventSetPlayerName.Raise(NetworkManager.Singleton.LocalClientId, playerNameSearchInputField.text);
            CustomNetworkDiscovery.instance.addresses.Clear();
            CustomNetworkDiscovery.instance.StopBroadcast();
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
            playersConnected++;
            UpdateWaitingForPlayersDialogInfo();

            foreach (CardTemplatePawn ctp in enterRoomCardTemplatePawnList)
            {
                foreach(string s in pawnNames)
                {
                    if (ctp._PawnName == s)
                    {
                        ctp.gameObject.SetActive(false);
                        if (selectedPawn?.name == s)
                            enterRoomPlayButton.interactable = false;
                    }
                }
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
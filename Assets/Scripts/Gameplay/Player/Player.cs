﻿using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using UnityEngine;
using TamborGame.Events;
using TamborGame.Utilities;
using TamborGame.Settings;

//Has an id
//Has a pawn
//Has a bool to check if can roll dices
//Has events
//Has lists of cards of people, practice and place
//Has network variables: lists of cards' ids of people, practice and place and a bool to check turn

//Roll dices when is it's turn
//Add to lists of cards and cards' ids
//Change turns

namespace TamborGame.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Player : NetworkBehaviour
    {
        //Runtime fields
        [HideInInspector]
        public bool canRollDices = false;
        List<Card> cardPersonList = new List<Card>(),
            cardPracticeList = new List<Card>(),
            cardPlaceList = new List<Card>(),
            discardedCardList = new List<Card>();
        [HideInInspector]
        public BoardSpace currentBoardSpace = null;

        //Inspector reference fields
        [SerializeField]
        GameEvent eventRequestPaths = null,
            eventRequestExtraCard = null,
            eventChangePlayerTurn = null,
            eventPlayersReady = null,
            eventStartGame = null,
            eventDisplayDiceResults = null,
            eventUpdateAvailablePawns = null,
            eventDiscardCard = null,
            eventUpdateStatusPanel = null,
            eventOpenExtraCardScreen = null,
            eventPlayerDisconnected = null;
        [SerializeField]
        SpriteRenderer spriteRenderer = null;
        [SerializeField]
        PawnContainer pawnContainer = null;

        //Properties
        public List<Card> _CardPersonList { get => cardPersonList; }
        public List<Card> _CardPracticeList { get => cardPracticeList; }
        public List<Card> _CardPlaceList { get => cardPlaceList; }

        //Network variables
        NetworkList<int> listPeopleIds = new NetworkList<int>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });
        NetworkList<int> listPracticesIds = new NetworkList<int>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });
        NetworkList<int> listPlacesIds = new NetworkList<int>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });
        NetworkList<int> discardedCardsIds = new NetworkList<int>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });
        [HideInInspector]
        public NetworkVariableBool isMyTurn = new NetworkVariableBool(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        }, false);
        public NetworkVariableBool isPlaying = new NetworkVariableBool(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        }, false);
        [HideInInspector]
        public NetworkVariableVector3 networkPosition = new NetworkVariableVector3(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });
        [HideInInspector]
        public NetworkVariableString pawnName = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });
        [HideInInspector]
        public NetworkVariableString playerName = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });

        private void OnEnable()
        {
            listPeopleIds.OnListChanged += AddPersonCardToListById;
            listPlacesIds.OnListChanged += AddPlaceCardToListById;
            listPracticesIds.OnListChanged += AddPracticeCardToListById;
            discardedCardsIds.OnListChanged += AddDiscardedCardToListById;
            isMyTurn.OnValueChanged += OnMyTurnValueChanged;
            pawnName.OnValueChanged += OnPawnNameValueChanged;
        }

        private void OnDisable()
        {
            listPeopleIds.OnListChanged -= AddPersonCardToListById;
            listPlacesIds.OnListChanged -= AddPlaceCardToListById;
            listPracticesIds.OnListChanged -= AddPracticeCardToListById;
            discardedCardsIds.OnListChanged -= AddDiscardedCardToListById;
            isMyTurn.OnValueChanged -= OnMyTurnValueChanged;
            pawnName.OnValueChanged -= OnPawnNameValueChanged;
        }

        private void Start()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
            Board.instance.gameObject.SetActive(true);
            currentBoardSpace = Board.instance.initialSpace;
            Board.instance.gameObject.SetActive(false);
        }

        public override void NetworkStart()
        {
            networkPosition.Value = transform.position;
            isMyTurn.Value = false;
            isPlaying.Value = true;
        }

        private void OnDestroy()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        }

        private void Singleton_OnClientDisconnectCallback(ulong obj)
        {
            if (OwnerClientId == obj)
            {
                eventPlayerDisconnected.Raise(obj);
                Destroy(gameObject);
            }
        }

        public void RollDices()
        {
            DiceProvider.RollDices();
            canRollDices = false;
            eventDisplayDiceResults.Raise();
        }

        public void ActivateReroll()
        {
            canRollDices = true;
            RollDices();
        }

        public void OnActUponDiceResults()
        {
            if (isMyTurn.Value && IsOwner)
            {
                if (DiceProvider._Dice1Result != DiceProvider._Dice2Result)
                    eventRequestPaths.Raise(DiceProvider._Dice1Result, DiceProvider._Dice2Result, currentBoardSpace);
                else
                {
                    eventOpenExtraCardScreen.Raise();
                    eventRequestExtraCard.Raise();
                }
            }
        }

        public void SetPlayerPawn(ulong clientID, string pawnName)
        {
            if (OwnerClientId == clientID)
                this.pawnName.Value = pawnName;
        }

        public void SetPlayerName(ulong clientID, string playerName)
        {
            if (OwnerClientId == clientID)
                this.playerName.Value = playerName;
        }

        public string[] GetPlayersPawnNames()
        {
            if (IsServer)
            {
                string[] pawnNames = new string[NetworkManager.Singleton.ConnectedClientsList.Count];
                List<MLAPI.Connection.NetworkClient> clientList = NetworkManager.Singleton.ConnectedClientsList;

                for (int i = 0; i < clientList.Count; i++)
                    pawnNames[i] = clientList[i].PlayerObject.GetComponent<Player>().pawnName.Value;

                return pawnNames;
            }

            return null;
        }

        public void AddToCardLists(Card card)
        {
            if (card.type == CardType.person)
                listPeopleIds.Add(card.id);
            else if (card.type == CardType.practice)
                listPracticesIds.Add(card.id);
            else if (card.type == CardType.place)
                listPlacesIds.Add(card.id);
        }

        public void AddToDiscardedCards(Card card)
        {
            if (!discardedCardList.Contains(card))
                discardedCardsIds.Add(card.id);
        }

        public bool IsItADiscardedCard(Card card)
        {
            return discardedCardList.Contains(card);
        }

        void AddPersonCardToListById(NetworkListEvent<int> changeEvent)
        {
            AddCardToListByIdAndType(changeEvent.Value, CardType.person);
        }

        void AddPlaceCardToListById(NetworkListEvent<int> changeEvent)
        {
            AddCardToListByIdAndType(changeEvent.Value, CardType.place);
        }

        void AddPracticeCardToListById(NetworkListEvent<int> changeEvent)
        {
            AddCardToListByIdAndType(changeEvent.Value, CardType.practice);
        }

        void AddCardToListByIdAndType(int cardId, CardType cardType)
        {
            List<Card> cardList = cardType == CardType.place ? cardPlaceList : cardType == CardType.person ? cardPersonList : cardPracticeList;
            Card c = CardProvider.instance.GetCardByTypeAndID(cardType, cardId);
            if (!cardList.Contains(c))
            {
                cardList.Add(c);
                if (IsOwner)
                    eventDiscardCard.Raise(c);
            }
        }

        void AddDiscardedCardToListById(NetworkListEvent<int> changeEvent)
        {
            Card c = CardProvider.instance.GetCardByID(changeEvent.Value);
            if (!discardedCardList.Contains(c))
            {
                discardedCardList.Add(c);
                if (IsOwner)
                    eventDiscardCard.Raise(c);
            }
        }

        void OnMyTurnValueChanged(bool previous, bool current)
        {
            if (current)
            {
                if (IsOwner && !canRollDices)
                    canRollDices = true;

                if (isMyTurn.Value && IsOwner && canRollDices)
                    RollDices();
            }

            if (IsOwner)
                eventUpdateStatusPanel.Raise(current);

        }

        void OnPawnNameValueChanged(string previous, string current)
        {
            if (!string.IsNullOrEmpty(current))
                spriteRenderer.sprite = pawnContainer.GetPawnByName(pawnName.Value).spritePawn;

            if (IsServer)
                UpdateAvailablePawnsClientRPC(GetPlayersPawnNames());
        }

        [ClientRpc]
        void UpdateAvailablePawnsClientRPC(string[] pawnNames)
        {
            eventUpdateAvailablePawns.Raise(pawnNames);
        }

        public void DisconnectPlayer()
        {
            if (OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                if (IsServer)
                    NetworkManager.Shutdown();
                else
                    DisconnectPlayerServerRPC(NetworkManager.Singleton.LocalClientId);
            }
        }

        [ServerRpc]
        public void DisconnectPlayerServerRPC(ulong clientID)
        {
            NetworkManager.Singleton.DisconnectClient(clientID);
        }

        public void SendStartGameRPC()
        {
            if (IsServer && IsOwner)
                StartGameClientRpc();
        }

        [ClientRpc]
        void StartGameClientRpc()
        {
            eventPlayersReady.Raise();
            eventStartGame.Raise();
        }

        public void ChangePlayerTurn()
        {
            if (IsServer && IsOwner)
                eventChangePlayerTurn.Raise();
            else if (!IsServer && IsOwner)
                ChangePlayerTurnServerRpc();
        }

        [ServerRpc]
        void ChangePlayerTurnServerRpc()
        {
            eventChangePlayerTurn.Raise();
        }
    }
}
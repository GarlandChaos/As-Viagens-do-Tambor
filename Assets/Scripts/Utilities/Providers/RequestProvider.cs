using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Connection;
using TamborGame;
using TamborGame.Gameplay;
using TamborGame.Events;

namespace TamborGame.Utilities
{
    public class RequestProvider : NetworkBehaviour
    {
        //Inspector reference fields
        [SerializeField]
        Player player = null;
        [SerializeField]
        GameEvent eventSendAvailablePawnsToRoomManager = null, eventSendCard = null, eventShowCardToPlayer = null, eventAskForGuessConfirmation = null,
            eventOpenGameOverScreen = null, eventGameOver = null, eventOpenPlayerLoseScreen = null, eventPlayerLose = null,
            eventSendEnvelopeCardsFromServer = null;

        public void OnRequestPawns()
        {
            if (!IsServer && IsOwner)
                RequestPawnsServerRpc();
        }

        [ServerRpc]
        void RequestPawnsServerRpc()
        {
            CreatePawnCardsClientRpc(player.GetPlayersPawnNames());
        }

        [ClientRpc]
        void CreatePawnCardsClientRpc(string[] pawnNames)
        {
            if (IsServer)
                return;

            eventSendAvailablePawnsToRoomManager.Raise(pawnNames);
        }

        public void OnRequestPersonCards()
        {
            if (IsOwner)
                RequestPlayerCardsServerRPC();
        }

        public void OnRequestPracticeCards()
        {
            if (IsOwner)
                RequestPracticeCardsServerRPC();
        }

        [ServerRpc]
        void RequestPlayerCardsServerRPC()
        {
            List<Card> cards = CardProvider.instance.GetPersonCards();
            int[] ids = new int[cards.Count];
            int i = 0;
            foreach (Card c in cards)
                ids[i++] = c.id;

            SendCardsClientRPC(ids);
        }

        [ServerRpc]
        void RequestPracticeCardsServerRPC()
        {
            List<Card> cards = CardProvider.instance.GetPracticeCards();
            int[] ids = new int[cards.Count];
            int i = 0;
            foreach (Card c in cards)
                ids[i++] = c.id;

            SendCardsClientRPC(ids);
        }

        [ClientRpc]
        void SendCardsClientRPC(int[] ids)
        {
            foreach (int i in ids)
                eventSendCard.Raise(CardProvider.instance.GetCardByID(i));
        }

        public void OnRequestToCheckGuessCards(int placeCardId, int personCardId, int practiceCardId)
        {
            if (IsOwner)
                CheckGuessCardsServerRPC(placeCardId, personCardId, practiceCardId);
        }

        [ServerRpc]
        void CheckGuessCardsServerRPC(int placeCardId, int personCardId, int practiceCardId)
        {
            List<Card> cards =
                CardProvider.instance.CheckGuessCards(CardProvider.instance.GetCardByID(placeCardId), CardProvider.instance.GetCardByID(personCardId), CardProvider.instance.GetCardByID(practiceCardId));

            if (cards.Count != 0)
                SendUnraveledCardClientRPC(cards[0].id, GameManager.instance.unraveledCardPlayerName);
            else
                SendUnraveledCardClientRPC();
        }

        [ClientRpc]
        void SendUnraveledCardClientRPC(int unraveledCardId = -1, string unraveledCardPlayerName = null)
        {
            if (unraveledCardId != -1)
            {
                Card card = CardProvider.instance.GetCardByID(unraveledCardId);
                eventShowCardToPlayer.Raise(card, unraveledCardPlayerName);
                player.AddToDiscardedCards(card);
            }
            else
                eventAskForGuessConfirmation.Raise();
        }

        public void OnRequestToCheckEnvelope(int placeCardId, int personCardId, int practiceCardId)
        {
            if (IsOwner)
                CheckEnvelopeServerRPC(placeCardId, personCardId, practiceCardId);
        }

        [ServerRpc]
        void CheckEnvelopeServerRPC(int placeCardId, int personCardId, int practiceCardId)
        {
            if (CardProvider.instance.CheckEnvelope(CardProvider.instance.GetCardByID(placeCardId), CardProvider.instance.GetCardByID(personCardId), CardProvider.instance.GetCardByID(practiceCardId)))
                GameOverClientRPC(player.playerName.Value); //cliente venceu
            else
                RemovePlayerFromGame(player);
        }

        void RemovePlayerFromGame(Player p)
        {
            p.isPlaying.Value = false;
            if (GetPlayerCount() > 1)
                PlayerLoseClientRPC(p.playerName.Value); //cliente perdeu
            else
                GameOverClientRPC(GetRemainingPlayer().playerName.Value); //se tiver apenas um, manda evento de vitória
        }

        public void OnHostWin()
        {
            if (IsServer && IsOwner)
                GameOverClientRPC(player.playerName.Value);
        }

        [ClientRpc]
        void GameOverClientRPC(string victorName)
        {
            eventOpenGameOverScreen.Raise();
            eventGameOver.Raise(victorName);
        }

        public void OnHostLose()
        {
            if (IsServer && IsOwner)
                RemovePlayerFromGame(player);
        }

        [ClientRpc]
        void PlayerLoseClientRPC(string loserName)
        {
            eventOpenPlayerLoseScreen.Raise();
            eventPlayerLose.Raise(loserName);
            player.ChangePlayerTurn();
        }

        int GetPlayerCount()
        {
            int playerCount = 0;
            if (IsServer)
            {
                foreach (NetworkClient n in NetworkManager.Singleton.ConnectedClientsList)
                {
                    Player p = n.PlayerObject.GetComponent<Player>();
                    if (p != null)
                        if (p.isPlaying.Value)
                            playerCount++;
                }
            }

            return playerCount;
        }

        Player GetRemainingPlayer()
        {
            if (IsServer)
            {
                foreach (NetworkClient n in NetworkManager.Singleton.ConnectedClientsList)
                {
                    Player p = n.PlayerObject.GetComponent<Player>();
                    if (p != null)
                        if (p.isPlaying.Value)
                            return p;
                }
            }

            return null;
        }

        public void OnRequestEnvelopeCards()
        {
            if (IsOwner)
                RequestEnvelopeCardsServerRPC();
        }

        [ServerRpc]
        void RequestEnvelopeCardsServerRPC()
        {
            RequestEnvelopeCardsClientRPC(CardProvider.instance._EnvelopePlace, CardProvider.instance._EnvelopePerson, CardProvider.instance._EnvelopePractice);
        }

        [ClientRpc]
        void RequestEnvelopeCardsClientRPC(int placeCardId, int personCardId, int practiceCardId)
        {
            eventSendEnvelopeCardsFromServer.Raise(placeCardId, personCardId, practiceCardId);
        }
    }
}
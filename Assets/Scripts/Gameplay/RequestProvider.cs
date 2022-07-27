using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

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
        List<Card> cards = GameManager.instance.GetPersonCards();
        int[] ids = new int[cards.Count];
        int i = 0;
        foreach (Card c in cards)
            ids[i++] = c.id;

        SendCardsClientRPC(ids);
    }

    [ServerRpc]
    void RequestPracticeCardsServerRPC()
    {
        List<Card> cards = GameManager.instance.GetPracticeCards();
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
            eventSendCard.Raise(GameManager.instance.GetCardByID(i));
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
            GameManager.instance.CheckGuessCards(GameManager.instance.GetCardByID(placeCardId), GameManager.instance.GetCardByID(personCardId), GameManager.instance.GetCardByID(practiceCardId));

        if (cards.Count != 0)
            SendUnraveledCardClientRPC(cards[0].id, GameManager.instance._UnraveledCardPlayerName);
        else
            SendUnraveledCardClientRPC();
    }

    [ClientRpc]
    void SendUnraveledCardClientRPC(int unraveledCardId = -1, string unraveledCardPlayerName = null)
    {
        if (unraveledCardId != -1)
        {
            Card card = GameManager.instance.GetCardByID(unraveledCardId);
            eventShowCardToPlayer.Raise(card, unraveledCardPlayerName);
            player.AddToDiscardedCards(card);
        }
        else
            eventAskForGuessConfirmation.Raise();
    }

    public void OnRequestToCheckEnvelope(int placeCardId, int personCardId, int practiceCardId)
    {
        if(IsOwner)
            CheckEnvelopeServerRPC(placeCardId, personCardId, practiceCardId);
    }

    [ServerRpc]
    void CheckEnvelopeServerRPC(int placeCardId, int personCardId, int practiceCardId)
    {
        if(GameManager.instance.CheckEnvelope(GameManager.instance.GetCardByID(placeCardId), GameManager.instance.GetCardByID(personCardId), GameManager.instance.GetCardByID(practiceCardId)))
            GameOverClientRPC(player.playerName.Value); //cliente venceu
        else
            PlayerLoseClientRPC(player.playerName.Value); //cliente perdeu
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
            PlayerLoseClientRPC(player.playerName.Value);
    }

    [ClientRpc]
    void PlayerLoseClientRPC(string loserName)
    {
        eventOpenPlayerLoseScreen.Raise();
        eventPlayerLose.Raise(loserName);
    }

    public void OnRequestEnvelopeCards()
    {
        if(IsOwner)
            RequestEnvelopeCardsServerRPC();
    }

    [ServerRpc]
    void RequestEnvelopeCardsServerRPC()
    {
        RequestEnvelopeCardsClientRPC(GameManager.instance._EnvelopePlace, GameManager.instance._EnvelopePerson, GameManager.instance._EnvelopePractice);
    }

    [ClientRpc]
    void RequestEnvelopeCardsClientRPC(int placeCardId, int personCardId, int practiceCardId)
    {
        eventSendEnvelopeCardsFromServer.Raise(placeCardId, personCardId, practiceCardId);
    }
}

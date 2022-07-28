using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.UI;
using MLAPI;
using MLAPI.Connection;
using System.Linq;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    //Singleton
    public static GameManager instance = null;

    //Inspector reference fields
    [SerializeField]
    GameObject prefabUIManager = null;
    [SerializeField]
    GameEvent eventShowCardToPlayer = null, eventAskForGuessConfirmation = null, eventShowExtraCard = null, eventSendExtraCardToAskIfWantToGoToPlaceScreen = null,
        eventRequestToCheckGuessCards = null, eventRequestToCheckEnvelope = null, eventWin = null, eventLose = null;
    [SerializeField]
    CardContainer extraCardsContainer = null;
    [SerializeField]
    GameObject boardGO = null;

    //Runtime fields
    Card currentPlaceGuess = null, currentPersonGuess = null, currentPracticeGuess = null;
    int turnPlayerIndex = 0;
    [HideInInspector]
    public string unraveledCardPlayerName = string.Empty;
    bool playerCanInteract = false;
    bool onGuessScreen = false;
    [HideInInspector]
    public bool DEBUGROOM = true; //PROVISÓRIO
    [HideInInspector]
    public bool DEBUGEXTRACARD = false; //PROVISÓRIO

    //Properties
    public int _TurnPlayerIndex { get => turnPlayerIndex; }
    public bool _PlayerCanInteract { get => playerCanInteract; }
    public Card _CurrentPlaceGuess { get => currentPlaceGuess; }
    public Card _CurrentPersonGuess { get => currentPersonGuess; }
    public Card _CurrentPracticeGuess { get => currentPracticeGuess; }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            Instantiate(prefabUIManager);
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void OnStartGame()
    {
        boardGO.SetActive(true);
        playerCanInteract = true;
        
        if (NetworkManager.Singleton.IsServer)
        {
            CardProvider.instance.OrganizeCards();
            NetworkManager.Singleton.ConnectedClientsList[turnPlayerIndex].PlayerObject.GetComponent<Player>().isMyTurn.Value = true;
        }
    }

    public void ResetFields(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            boardGO.SetActive(false);
            currentPlaceGuess = null;
            currentPersonGuess = null;
            currentPracticeGuess = null;
            turnPlayerIndex = 0;
            unraveledCardPlayerName = string.Empty;
            playerCanInteract = false;
            onGuessScreen = false;
        }
    }

    void ChangePlayerTurn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Player p = NetworkManager.Singleton.ConnectedClientsList[turnPlayerIndex].PlayerObject.GetComponent<Player>();
            p.isMyTurn.Value = false;

            do
            {
                turnPlayerIndex = (turnPlayerIndex + 1) % NetworkManager.Singleton.ConnectedClientsList.Count;
                p = NetworkManager.Singleton.ConnectedClientsList[turnPlayerIndex].PlayerObject.GetComponent<Player>();
            }
            while (!p.isPlaying.Value);
            
            p.isMyTurn.Value = true;
        }
    }

    public void OnChangePlayerTurn()
    {
        ChangePlayerTurn();
    }

    public void OnRequestExtraCard()
    {
        eventShowExtraCard.Raise(extraCardsContainer._Cards[Random.Range(0, extraCardsContainer._Cards.Count)]);
    }

    public void OnAskIfWantToGuess(Card cardPlace)
    {
        currentPlaceGuess = cardPlace;
        playerCanInteract = false;
        onGuessScreen = true;
    }

    public void OnCloseGuessScreen()
    {
        playerCanInteract = true;
        onGuessScreen = false;
    }

    public void SetGuessCardsAndCheck(Card personCard, Card practiceCard)
    {
        currentPersonGuess = personCard;
        currentPracticeGuess = practiceCard;
        List<Card> cardsToShow = CardProvider.instance.CheckGuessCards(currentPlaceGuess, personCard, practiceCard);

        if(cardsToShow != null) //is host
        {
            if (cardsToShow.Count == 0)
                eventAskForGuessConfirmation.Raise(); //ninguém tinha as cartas, pergunta para o jogador do turno se quer confirmar o palpite
            else
            {
                eventShowCardToPlayer.Raise(cardsToShow[0], unraveledCardPlayerName);
                NetworkManager.Singleton.ConnectedClientsList[turnPlayerIndex].PlayerObject.GetComponent<Player>().AddToDiscardedCards(cardsToShow[0]);
            }
        }
        else //is client
            eventRequestToCheckGuessCards.Raise(currentPlaceGuess.id, currentPersonGuess.id, currentPracticeGuess.id);
    }

    public void OnTryToWinWithGuess()
    {
        if (NetworkManager.Singleton.IsServer) //is server
        {
            if (CardProvider.instance.CheckEnvelope(currentPlaceGuess, currentPersonGuess, currentPracticeGuess)) //local player win
                eventWin.Raise(); //manda ClientRPC para todos encerrando o jogo
            else //local player lose
                eventLose.Raise(); //manda ClientRPC e assiste o jogo até o fim
        }
        else //is client
            eventRequestToCheckEnvelope.Raise(currentPlaceGuess.id, currentPersonGuess.id, currentPracticeGuess.id);
    }

    public void OnEffectGoToPlaceOptional(ExtraCard extraCard)
    {
        eventSendExtraCardToAskIfWantToGoToPlaceScreen.Raise(extraCard);
    }

    public void OnPlayerCanInteract()
    {
        if (!onGuessScreen)
            playerCanInteract = true;
    }

    public void OnPlayerCannotInteract()
    {
        if (!onGuessScreen)
            playerCanInteract = false;
    }
}
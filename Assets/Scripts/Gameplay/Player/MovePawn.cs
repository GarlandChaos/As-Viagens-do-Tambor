using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using TamborGame.Events;
using TamborGame.Utilities;
using TamborGame.Settings;

namespace TamborGame.Gameplay
{
    public class MovePawn : NetworkBehaviour
    {
        //Inspector reference fields
        [SerializeField]
        Player player = null;
        [SerializeField]
        GameEvent eventEndOfMove = null, eventAskIfWantToGuess = null, eventOpenGuessScreen = null;
        [SerializeField]
        InterpolationSettings animationSettings = null;

        public void OnFinalBoardSpacePressed(Queue<BoardSpace> path)
        {
            MovePlayerPawn(path, true);
        }

        public void MovePlayerPawn(Queue<BoardSpace> path, bool askIfWantToGuess)
        {
            if (player.isMyTurn.Value && IsOwner)
            {
                if (IsServer)
                    MovePawnClientRpc(Board.instance.GetPathIds(path), NetworkManager.Singleton.LocalClientId, askIfWantToGuess); //send client rpc to move
                else
                    MovePawnServerRpc(Board.instance.GetPathIds(path), NetworkManager.Singleton.LocalClientId, askIfWantToGuess); //send server rpc to move
            }
        }

        [ServerRpc]
        void MovePawnServerRpc(int[] ids, ulong clientID, bool askIfWantToGuess)
        {
            MovePawnClientRpc(ids, clientID, askIfWantToGuess);
        }

        [ClientRpc]
        void MovePawnClientRpc(int[] ids, ulong clientID, bool askIfWantToGuess)
        {
            bool ask = OwnerClientId == clientID ? askIfWantToGuess : false;
            StartCoroutine(MoveCoroutine(Board.instance.GetBoardSpacesByIds(ids), ask));
        }

        public void MoveToPlaceFromExtraCard(ExtraCard extraCard)
        {
            if (player.isMyTurn.Value && IsOwner)
            {
                BoardSpace goal = Board.instance.GetPlaceByEnum(extraCard.placeToGo);
                MovePlayerPawn(AStar.CalculatePathToPlace(player.currentBoardSpace, goal), extraCard.askIfWantToGuess);
            }
        }

        IEnumerator MoveCoroutine(Queue<BoardSpace> path, bool askIfWantToGuess)
        {
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            float timer = 0f;
            Vector3 startPos = transform.position;
            BoardSpace bs = path.Dequeue();
            Vector3 endPos = bs.transform.position;

            while (timer < 1f)
            {
                timer += Time.deltaTime * animationSettings._Duration;
                transform.position = Vector3.Lerp(startPos, endPos, animationSettings._Curve.Evaluate(timer));

                yield return wait;
            }

            if (path.Count > 0)
                StartCoroutine(MoveCoroutine(path, askIfWantToGuess));
            else
            {
                player.currentBoardSpace = bs;
                if (IsOwner)
                {
                    eventEndOfMove.Raise();
                    Place place = bs.GetComponent<Place>();
                    if (place != null)
                    {
                        if (askIfWantToGuess)
                        {
                            eventOpenGuessScreen.Raise();
                            eventAskIfWantToGuess.Raise(place.cardPlace);
                        }
                        else
                            player.ChangePlayerTurn();
                    }
                    else
                        player.ChangePlayerTurn();
                }
            }
        }
    }
}
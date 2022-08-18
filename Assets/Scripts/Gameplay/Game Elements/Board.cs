using System.Collections.Generic;
using UnityEngine;
using TamborGame.Events;

//This class currently do:
// - act as a singleton;
// - initializes boards ids;
// - clear pathList and reset boards color;
// - calculate paths and add to pathList;
// - attach FinalBoardSpace script to boards;
// - hold the FinalBoardSpace pressed/selected/deselected callbacks;
// - return path ids;
// - return board spaces by id checking
// - return place by enum checking

namespace TamborGame.Gameplay
{
    public class Board : MonoBehaviour
    {
        //Singleton
        public static Board instance = null;

        //Inspector reference fields
        public List<Place> places = new List<Place>();
        public BoardSpace initialSpace = null;

        //Runtime fields
        public List<BoardSpace> spaces = new List<BoardSpace>();
        public List<Queue<BoardSpace>> pathList = new List<Queue<BoardSpace>>();

        //Serialized fields
        [SerializeField]
        GameEvent eventFinalBoardSpaceSelected = null,
                  eventFinalBoardSpaceDeselected = null,
                  eventFinalBoardSpacePressed = null;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        void Start()
        {
            BoardSpace[] bs = gameObject.GetComponentsInChildren<BoardSpace>();
            int id = 0;
            foreach (BoardSpace b in bs)
            {
                b.id = id;
                spaces.Add(b);
                id++;
            }

            gameObject.SetActive(false);
        }

        public void OnRequestPaths(int dice1, int dice2, BoardSpace bs)
        {
            ClearPathList();
            CalculatePaths(bs, dice1 + dice2, pathList);
            ChangePathColor();
        }

        public void ChangePathColor()
        {
            foreach (Queue<BoardSpace> q in pathList)
            {
                foreach (BoardSpace b in q)
                    b._Marker.SetActive(true);
            }
        }

        public void ClearPathList()
        {
            foreach (Queue<BoardSpace> q in pathList)
            {
                foreach (BoardSpace b in q)
                    b._Marker.SetActive(false);
            }
            pathList.Clear();
        }

        public void CalculatePaths(BoardSpace boardSpace, int moves, List<Queue<BoardSpace>> pathList, Queue<BoardSpace> path = null)
        {
            foreach (BoardSpace bs in boardSpace._Adjacent)
            {
                Queue<BoardSpace> p;
                int m = moves;
                if (path != null)
                    p = new Queue<BoardSpace>(path);
                else
                    p = new Queue<BoardSpace>();

                if (!p.Contains(boardSpace)) //check if the current space isn't in the path so the player don't go back
                    p.Enqueue(boardSpace);

                if (!p.Contains(bs)) //check if the adjacent space isn't in the path so the player don't repeat moves
                {
                    p.Enqueue(bs);
                    m--;
                    if (m > 0 && !bs.GetComponent<Place>())
                        CalculatePaths(bs, m, pathList, p);
                    else
                    {
                        pathList.Add(p); //save the completed path
                        if (bs.gameObject.GetComponent<FinalBoardSpace>() == null) //add script to final board Space if it hasn't one already, passing the complete path
                            FinalBoardSpace.Attach(bs.gameObject, eventFinalBoardSpaceSelected, eventFinalBoardSpaceDeselected, eventFinalBoardSpacePressed, p);
                    }
                }
            }
        }

        public void OnFinalBoardSpaceSelected(Queue<BoardSpace> path) //DÁ PRA MELHORAR...
        {
            foreach (Queue<BoardSpace> q in pathList)
            {
                foreach (BoardSpace b in q)
                {
                    if (!path.Contains(b))
                        b._Marker.SetActive(false);
                }
            }
        }

        public void OnFinalBoardSpacePressed()
        {
            foreach (Place p in places)
            {
                FinalBoardSpace fbs = p.GetComponent<FinalBoardSpace>();
                if (fbs != null)
                    Destroy(fbs);
            }
        }

        public int[] GetPathIds(Queue<BoardSpace> path)
        {
            Queue<BoardSpace> p = new Queue<BoardSpace>(path);
            int n = path.Count;
            int[] ids = new int[n];
            for (int i = 0; i < n; i++)
                ids[i] = p.Dequeue().id;

            return ids;
        }

        public Queue<BoardSpace> GetBoardSpacesByIds(int[] ids)
        {
            Queue<BoardSpace> boardSpaces = new Queue<BoardSpace>();
            for (int i = 0; i < ids.Length; i++)
            {
                foreach (BoardSpace bs in spaces)
                {
                    if (bs.id == ids[i])
                    {
                        boardSpaces.Enqueue(bs);
                        break;
                    }
                }
            }

            return boardSpaces;
        }

        public BoardSpace GetPlaceByEnum(PlaceName placeName)
        {
            foreach (Place p in places)
            {
                if (p.placeName == placeName)
                    return p.GetComponent<BoardSpace>();
            }

            return null;
        }
    }
}
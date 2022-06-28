﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class Board : MonoBehaviour
{
    //Singleton
    public static Board instance;

    //Inspector references
    public List<Place> places;
    public Material pathMat, finalBoardSpaceMat, localMat, boardSpaceMat;
    public BoardSpace initialSpace;

    //Runtime fields
    public List<BoardSpace> spaces = new List<BoardSpace>();
    public List<Queue<BoardSpace>> pathList = new List<Queue<BoardSpace>>();

    //Serialized fields
    [SerializeField]
    GameEvent eventFinalBoardSpaceSelected, 
              eventFinalBoardSpaceDeselected, 
              eventFinalBoardSpacePressed;

    // Start is called before the first frame update
    void Start()
    {
        if(instance != null)
            Destroy(this);
        else
            instance = this;

        BoardSpace[] bs = gameObject.GetComponentsInChildren<BoardSpace>();
        int id = 0;
        foreach(BoardSpace b in bs)
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
            int i = 0;
            foreach (BoardSpace b in q)
            {
                SpriteRenderer bSpriteRenderer = b.GetComponent<SpriteRenderer>();
                if (i < q.Count - 1)
                    bSpriteRenderer.color = pathMat.color;
                else
                    bSpriteRenderer.color = finalBoardSpaceMat.color;
                i++;
            }
        }
    }

    public void ClearPathList()
    {
        foreach (Queue<BoardSpace> q in pathList)
        {
            foreach (BoardSpace b in q)
                b.GetComponent<SpriteRenderer>().color = localMat.color;
        }
        pathList.Clear();
    }

    public void CalculatePaths(BoardSpace boardSpace, int moves, List<Queue<BoardSpace>> pathList, Queue<BoardSpace> path = null)
    {
        foreach(BoardSpace bs in boardSpace.adjacent)
        {
            Queue<BoardSpace> p;
            int m = moves;
            if(path != null)
                p = new Queue<BoardSpace>(path);
            else
                p = new Queue<BoardSpace>();
           
            if (!p.Contains(boardSpace)) //check if the current space isn't in the path so the player don't go back
                p.Enqueue(boardSpace);
            
            if (!p.Contains(bs)) //check if the adjacent space isn't in the path so the player don't repeat moves
            {
                p.Enqueue(bs);
                m--;
                if(m > 0 && !bs.GetComponent<Place>())
                    CalculatePaths(bs, m, pathList, p);
                else
                {
                    pathList.Add(p); //save the completed path
                    if(bs.gameObject.GetComponent<FinalBoardSpace>() == null) //add script to final board Space if it hasn't one already, passing the complete path
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
                    b.GetComponent<SpriteRenderer>().color = localMat.color;
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
        for(int i = 0; i < n; i++)
            ids[i] = p.Dequeue().id;

        return ids;
    }

    public Queue<BoardSpace> GetBoardSpacesByIds(int[] ids)
    {
        Queue<BoardSpace> boardSpaces = new Queue<BoardSpace>();
        for(int i = 0; i < ids.Length; i++)
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
        foreach(Place p in places)
        {
            if(p.placeName == placeName)
                return p.GetComponent<BoardSpace>();
        }

        return null;
    }
}
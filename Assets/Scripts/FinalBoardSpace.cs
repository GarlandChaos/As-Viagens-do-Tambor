using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoardSpace : MonoBehaviour
{
    [SerializeField]
    GameEvent eventFinalBoardSpaceSelected, eventFinalBoardSpaceDeselected, eventFinalBoardSpacePressed;
    [SerializeField]
    Queue<BoardSpace> completePath;

    public static FinalBoardSpace Attach(GameObject go, GameEvent fbSelected, GameEvent fbDeselected, GameEvent fbPressed, Queue<BoardSpace> path)
    {
        FinalBoardSpace fbs = go.AddComponent<FinalBoardSpace>();
        fbs.Setup(fbSelected, fbDeselected, fbPressed, path);

        return fbs;
    }

    private void Setup(GameEvent fbSelected, GameEvent fbDeselected, GameEvent fbPressed, Queue<BoardSpace> path)
    {
        eventFinalBoardSpaceSelected = fbSelected;
        eventFinalBoardSpaceDeselected = fbDeselected;
        eventFinalBoardSpacePressed = fbPressed;
        completePath = new Queue<BoardSpace>(path);
    }

    private void OnMouseEnter()
    {
        if (GameManager.instance._PlayerCanInteract)
        {
            eventFinalBoardSpaceSelected.Raise(completePath);
        }
    }

    private void OnMouseExit()
    {
        if (GameManager.instance._PlayerCanInteract)
        {
            eventFinalBoardSpaceDeselected.Raise();
        }
    }

    private void OnMouseDown()
    {
        if (GameManager.instance._PlayerCanInteract)
        {
            eventFinalBoardSpacePressed.Raise(completePath);
        }
    }
}

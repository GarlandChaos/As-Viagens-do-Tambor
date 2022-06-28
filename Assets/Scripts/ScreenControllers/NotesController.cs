using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotesController : APanelController
{
    List<NoteCard> noteCards = new List<NoteCard>();
    [SerializeField]
    Button notePanel = null;
    bool hidden = true;
    [SerializeField]
    GameEvent eventPlayerCanInteract = null, eventPlayerCannotInteract = null;

    private void Awake()
    {
        foreach (NoteCard n in GetComponentsInChildren<NoteCard>())
        {
            noteCards.Add(n);
        }
    }

    public void OnDiscardCards(Card card)
    {
        foreach(NoteCard n in noteCards)
        {
           n.CardCheck(card);
        }
    }

    public void ShowOrHidePanel()
    {
        //float x = notePanel.GetComponent<RectTransform>().sizeDelta.x / 2;
        RectTransform rt = notePanel.GetComponent<RectTransform>();
        //Vector3 pos = rt.anchoredPosition;
        if (hidden)
        {
            //rt.anchoredPosition = new Vector3(-x, pos.y, pos.z);
            //hidden = false;
            eventPlayerCannotInteract.Raise();
        }
        else
        {
            //rt.anchoredPosition = new Vector3(x - 120f, pos.y, pos.z);
            //hidden = true;
            eventPlayerCanInteract.Raise();
        }
        StartCoroutine(ShowOrHideCoroutine(rt));
    }

    IEnumerator ShowOrHideCoroutine(RectTransform rt)
    {
        notePanel.interactable = false;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        float timer = 0f;
        float duration = 0.5f;
        float x = rt.sizeDelta.x / 2;
        Vector3 startPos = rt.anchoredPosition;
        Vector3 endPos = Vector3.zero;
        if (hidden)
        {
            endPos = new Vector3(-x, startPos.y, startPos.z);
            hidden = false;
            //eventPlayerCanInteract.Raise();
        }
        else
        {
            endPos = new Vector3(x - 120f, startPos.y, startPos.z);
            hidden = true;
            //eventPlayerCannotInteract.Raise();
        }
        Vector3 delta = endPos - startPos;

        do
        {
            timer += Time.deltaTime / duration;

            rt.anchoredPosition = startPos + delta * curve.Evaluate(timer);
            yield return wait;
        }
        while (timer < 1f);
        notePanel.interactable = true;
    }
}

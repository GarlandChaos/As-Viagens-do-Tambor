using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace System.UI
{
    public class NotesController : ASpecialPanelController
    {
        //Inspector reference fields
        [SerializeField]
        Button notePanel = null;
        [SerializeField]
        RectTransform arrowIndicator1 = null;
        [SerializeField]
        RectTransform arrowIndicator2 = null;
        [SerializeField]
        GameEvent eventPlayerCanInteract = null, eventPlayerCannotInteract = null;

        //Runtime fields
        List<NoteCard> noteCards = new List<NoteCard>();
        bool hidden = true;

        private void Awake()
        {
            foreach (NoteCard n in GetComponentsInChildren<NoteCard>())
                noteCards.Add(n);
        }

        public void OnDiscardCards(Card card)
        {
            foreach (NoteCard n in noteCards)
                n.CardCheck(card);
        }

        public void ShowOrHidePanel()
        {
            RectTransform rt = notePanel.GetComponent<RectTransform>();
            if (hidden)
                eventPlayerCannotInteract.Raise();
            else
                eventPlayerCanInteract.Raise();

            StartCoroutine(ShowOrHideCoroutine(rt));
        }

        void FlipImage(RectTransform image)
        {
            Vector3 newPos = image.anchoredPosition;
            newPos.x = newPos.x == 0 ? image.sizeDelta.x : 0;
            image.anchoredPosition = newPos;

            Vector3 newScale = image.localScale;
            newScale.x = -newScale.x;
            image.localScale = newScale;
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

                FlipImage(arrowIndicator1);
                FlipImage(arrowIndicator2);
            }
            else
            {
                endPos = new Vector3(x - 120f, startPos.y, startPos.z);
                hidden = true;

                FlipImage(arrowIndicator1);
                FlipImage(arrowIndicator2);
            }

            Vector3 delta = endPos - startPos;

            while (timer < 1f)
            {
                timer += Time.deltaTime / duration;
                rt.anchoredPosition = startPos + delta * curve.Evaluate(timer);

                yield return wait;
            }

            notePanel.interactable = true;
        }
    }
}
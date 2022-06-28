using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtraCardScreenController : ADialogController
{
    [SerializeField]
    Image rtExtraCardContainer = null;
    [SerializeField]
    Sprite cardBackSprite = null;
    ExtraCard currentExtraCard = null;
    [SerializeField]
    GameEvent eventEffectLoseTurn = null, eventEffectReroll = null, eventEffectGoToPlaceAndGuess = null, eventEffectGoToPlaceOptional = null;

    public void OnShowExtraCard(ExtraCard extraCard)
    {
        //rtExtraCardContainer.sprite = extraCard.sprite;
        currentExtraCard = extraCard;
        StartCoroutine(ShowExtraCardCoroutine(extraCard));
    }

    public void ConfirmExtraCardEffect()
    {
        switch (currentExtraCard.effect)
        {
            case Effect.reroll:
                eventEffectReroll.Raise();
                break;

            case Effect.loseTurn:
                eventEffectLoseTurn.Raise();
                break;

            case Effect.goToPlace:
#if UNITY_EDITOR
                Debug.Log("Effect goToPlaceAndGuess");
#endif
                //precisa comunicar o local exato que o player deve ir...
                //http://theory.stanford.edu/~amitp/GameProgramming/AStarComparison.html
                eventEffectGoToPlaceAndGuess.Raise(currentExtraCard);
                break;

            case Effect.goToPlaceOptional:
                eventEffectGoToPlaceOptional.Raise(currentExtraCard);
                break;

            case Effect.goToPublicMarketAndChoosePlace:

                break;

            case Effect.returnToPreviousPlaceAndGuess:

                break;

            default:
                break;
        }
    }

    IEnumerator ShowExtraCardCoroutine(ExtraCard extraCard)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        float timer = 0f;
        float animationTime = 1.5f;
        rtExtraCardContainer.sprite = cardBackSprite;
        Vector3 startScale = rtExtraCardContainer.rectTransform.localScale;
        Vector3 endScale = startScale;
        endScale.x = 0f;

        do
        {
            timer += Time.deltaTime / animationTime;
            yield return wait;
        }
        while (timer < 1f);

        animationTime = 0.25f;

        do
        {
            timer += Time.deltaTime / animationTime;
            rtExtraCardContainer.rectTransform.localScale = Vector3.Lerp(startScale, endScale, curve.Evaluate(timer));
            yield return wait;
        }
        while (timer < 1f);

        rtExtraCardContainer.sprite = extraCard.sprite;
        //currentExtraCard = extraCard;

        timer = 0f;
        endScale = startScale;
        startScale = rtExtraCardContainer.rectTransform.localScale;
        do
        {
            timer += Time.deltaTime / animationTime;
            rtExtraCardContainer.rectTransform.localScale = Vector3.Lerp(startScale, endScale, curve.Evaluate(timer));
            yield return wait;
        }
        while (timer < 1f); 
    }
}

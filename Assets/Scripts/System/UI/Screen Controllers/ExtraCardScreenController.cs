using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TamborGame.Gameplay;
using TamborGame.Events;
using TamborGame.Settings;

namespace TamborGame.UI
{
    public class ExtraCardScreenController : ADialogController
    {
        //Inspector reference fields
        [SerializeField]
        Image imageExtraCard = null;
        [SerializeField]
        Sprite spriteCardBack = null;
        ExtraCard currentExtraCard = null;
        [SerializeField]
        Button buttonConfirm = null;
        [SerializeField]
        GameEvent eventEffectLoseTurn = null, eventEffectReroll = null, eventEffectGoToPlaceAndGuess = null, eventOpenAskIfWantToGoToPlaceScreen = null,
            eventEffectGoToPlaceOptional = null, eventCloseExtraCardScreen = null;
        [SerializeField]
        InterpolationSettings animationSettings = null;

        public void OnShowExtraCard(ExtraCard extraCard)
        {
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
                    eventEffectGoToPlaceAndGuess.Raise(currentExtraCard);
                    eventCloseExtraCardScreen.Raise();
                    break;

                case Effect.goToPlaceOptional:
                    eventOpenAskIfWantToGoToPlaceScreen.Raise();
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
            buttonConfirm.interactable = false;
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            float timer = 0f;
            imageExtraCard.sprite = spriteCardBack;
            Vector3 startScale = imageExtraCard.rectTransform.localScale;
            Vector3 endScale = startScale;
            endScale.x = 0f;

            while (timer < 1f)
            {
                timer += Time.deltaTime / animationSettings._Duration;
                
                yield return wait;
            }

            float animationTime = 0.25f;
            timer = 0f;

            while (timer < 1f)
            {
                timer += Time.deltaTime / animationTime;
                imageExtraCard.rectTransform.localScale = Vector3.Lerp(startScale, endScale, animationSettings._Curve.Evaluate(timer));
                
                yield return wait;
            }

            imageExtraCard.sprite = extraCard.sprite;

            timer = 0f;
            endScale = startScale;
            startScale = imageExtraCard.rectTransform.localScale;
            
            while (timer < 1f)
            {
                timer += Time.deltaTime / animationTime;
                imageExtraCard.rectTransform.localScale = Vector3.Lerp(startScale, endScale, animationSettings._Curve.Evaluate(timer));
                
                yield return wait;
            }

            buttonConfirm.interactable = true;
        }
    }
}
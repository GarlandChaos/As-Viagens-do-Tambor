using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace System.UI
{
    public class DicesResultsController : APanelController
    {
        //Inspector references
        [SerializeField]
        Image imageDice1 = null;
        [SerializeField]
        Image imageDice2 = null;
        [SerializeField]
        Sprite[] diceFacesSprites = new Sprite[6];
        [SerializeField]
        GameObject buttonRollDice = null;
        [SerializeField]
        GameObject panelDice1 = null;
        [SerializeField]
        GameObject panelDice2 = null;
        [SerializeField]
        GameEvent eventActUponDiceResults = null;
        [SerializeField]
        RectTransform background1 = null, background2 = null;

        //Runtime field
        Dictionary<int, Sprite> diceFaceSpriteDictionary = new Dictionary<int, Sprite>();

        private void Awake()
        {
            for (int i = 0; i < diceFacesSprites.Length; i++)
                diceFaceSpriteDictionary.Add(i + 1, diceFacesSprites[i]);
        }

        private void OnEnable()
        {
            buttonRollDice.SetActive(true);
            panelDice1.SetActive(false);
            panelDice2.SetActive(false);
            RefreshPanel();
        }

        public async void OnRollDiceButton()
        {
            buttonRollDice.SetActive(false);
            panelDice1.SetActive(true);
            panelDice2.SetActive(true);
            RefreshPanel();

            await AnimateDicesResults();
            eventActUponDiceResults.Raise();
        }

        void RefreshPanel()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(background1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(background2);
        }

        IEnumerator AnimateDicesResults()
        {
            WaitForEndOfFrame wait = new WaitForEndOfFrame();

            for (int i = 1; i < diceFaceSpriteDictionary.Count + 1; i++)
            {
                if (Dices._Dice1Result != i)
                    imageDice1.sprite = diceFaceSpriteDictionary[i];


                if (Dices._Dice2Result != i)
                    imageDice2.sprite = diceFaceSpriteDictionary[i];

                yield return new WaitForSeconds(0.15f);
            }

            imageDice1.sprite = diceFaceSpriteDictionary[Dices._Dice1Result];
            imageDice2.sprite = diceFaceSpriteDictionary[Dices._Dice2Result];
        }
    }
}
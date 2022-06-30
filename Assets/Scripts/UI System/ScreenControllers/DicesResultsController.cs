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

        //Runtime field
        Dictionary<int, Sprite> diceFaceSpriteDictionary = new Dictionary<int, Sprite>();

        private void Awake()
        {
            for (int i = 0; i < diceFacesSprites.Length; i++)
                diceFaceSpriteDictionary.Add(i + 1, diceFacesSprites[i]);
        }

        private void OnEnable()
        {
            StartCoroutine(AnimateDicesResults());
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
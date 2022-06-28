using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DicesResultsController : APanelController
{
    [SerializeField]
    TMP_Text textDice1 = null;
    [SerializeField]
    TMP_Text textDice2 = null;

    private void OnEnable()
    {
        textDice1.text = GameManager.instance.currentDice1Result.ToString();
        textDice2.text = GameManager.instance.currentDice2Result.ToString();
    }
}

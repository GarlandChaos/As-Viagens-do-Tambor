using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dices : MonoBehaviour
{
    public static Dices instance;
    [SerializeField]
    GameEvent eventDisplayDicesResults;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int[] RollDices()
    {
        int[] dices = new int[2];
        dices[0] = Random.Range(1, 7);
        dices[1] = Random.Range(1, 7);
        eventDisplayDicesResults.Raise(dices[0], dices[1]);
        return dices;
    }
}

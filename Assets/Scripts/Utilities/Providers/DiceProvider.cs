using UnityEngine;

namespace TamborGame.Utilities
{
    public static class DiceProvider
    {
        //Runtime fields
        static int[] dicesResults = new int[2];

        //Properties
        public static int _Dice1Result { get => dicesResults[0]; }
        public static int _Dice2Result { get => dicesResults[1]; }

        public static int[] RollDices()
        {
            dicesResults[0] = Random.Range(1, 7);
            dicesResults[1] = Random.Range(1, 7);

            return dicesResults;
        }
    }
}
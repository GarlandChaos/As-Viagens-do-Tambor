using System.Collections.Generic;
using UnityEngine;

namespace TamborGame.Gameplay
{
    public class BoardSpace : MonoBehaviour
    {
        //Public fields
        public int id = -1;

        //Inspector reference fields
        [SerializeField]
        List<BoardSpace> adjacent = new List<BoardSpace>();
        [SerializeField]
        GameObject marker = null;

        //Properties
        public List<BoardSpace> _Adjacent { get => adjacent; }
        public GameObject _Marker { get => marker; }
    }
}
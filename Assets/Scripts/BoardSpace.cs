using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    public List<BoardSpace> adjacent = new List<BoardSpace>();
    public int id = -1;
    public GameObject marker = null;
}

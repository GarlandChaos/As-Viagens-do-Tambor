using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scaler : MonoBehaviour
{
    void Start()
    {
        transform.localScale = Vector3.one * Camera.main.orthographicSize * 1.8f;   
    }
}

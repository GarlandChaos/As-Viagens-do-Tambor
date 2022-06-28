using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomUITemplate : MonoBehaviour
{
    [SerializeField]
    TMP_Text roomNameText;
    [SerializeField]
    Button playButton;

    public void SetRoomName(string s)
    {
        roomNameText.text = s;
    }

    public Button GetPlayButton()
    {
        return playButton;
    }

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomTemplate : MonoBehaviour
{
    //Inspector reference fields
    [SerializeField]
    TMP_Text roomNameText = null;
    [SerializeField]
    Button playButton = null;

    public void SetRoomName(string s)
    {
        roomNameText.text = s;
    }

    public Button GetPlayButton()
    {
        return playButton;
    }
}

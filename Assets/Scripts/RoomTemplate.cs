using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomTemplate : MonoBehaviour
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
}

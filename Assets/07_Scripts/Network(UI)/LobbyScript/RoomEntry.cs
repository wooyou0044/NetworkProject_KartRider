using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomEntry : MonoBehaviour
{
    [SerializeField] public TMP_Text roomNumberText;
    [SerializeField] public TMP_Text roomNameText;
    [SerializeField] public TMP_Text playerCountText;
    [SerializeField] public TMP_Text MaxPlayersText;
    [SerializeField] public TMP_Text mapNameText;
    [SerializeField] public Image mapImage;
    [SerializeField] public Image lockIcon;
    [SerializeField] public GameObject roomNamePanel;    
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        // 방 정보 업데이트
        roomNumberText.text = roomInfo.CustomProperties.ContainsKey("RoomNumber")
            ? (string)roomInfo.CustomProperties["RoomNumber"]
            : "맵넘버?";
        roomNameText.text = roomInfo.CustomProperties.ContainsKey("RoomName")
            ? (string)roomInfo.CustomProperties["RoomName"]
            : "맵이름?";
        playerCountText.text = $"{roomInfo.PlayerCount}";
        MaxPlayersText.text = $"/ {roomInfo.MaxPlayers}";
        mapNameText.text = roomInfo.CustomProperties.ContainsKey("MapName")
            ? (string)roomInfo.CustomProperties["MapName"]
            : "기본 맵";

        // 맵 이미지 설정
        var mapSprite = Resources.Load<Sprite>($"Maps/{mapNameText.text}");
        mapImage.sprite = mapSprite != null ? mapSprite : Resources.Load<Sprite>("Maps/default");

        // 잠금 상태 설정
        lockIcon.enabled = !roomInfo.IsOpen;
    }
}


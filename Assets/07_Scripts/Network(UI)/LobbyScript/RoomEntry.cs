using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomEntry : MonoBehaviour
{
    [SerializeField] public TMP_Text roomNumberText;
    [SerializeField] public TMP_Text roomNameText;
    [SerializeField] public TMP_Text roomPasswordText;
    [SerializeField] public TMP_Text playerCountText;
    [SerializeField] public TMP_Text MaxPlayersText;
    [SerializeField] public TMP_Text mapNameText;
    [SerializeField] public Image mapImage;
    [SerializeField] public Image lockIcon;
    [SerializeField] public GameObject roomNamePanel;

    /// <summary>
    /// 방 정보를 로비화면에서 셋팅하기 위한 RoomEntry 
    /// 방을 생성하게 되면 방의 정보가 로비에 있는 사람들에게 보여줌
    /// </summary>
    /// <param name="roomInfo">포톤에서 제공해 주는 룸인포 정보를 받아옴</param>
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        //방 이름 받아서 처리, 방 이름 디폴트 값 방이름?
        roomNameText.text = roomInfo.CustomProperties.ContainsKey("RoomName")
            ? (string)roomInfo.CustomProperties["RoomName"]
            : "방이름?";
        //방넘버 
        roomNumberText.text = roomInfo.CustomProperties.ContainsKey("RoomNumber")
            ? (string)roomInfo.CustomProperties["RoomNumber"].ToString()
            : "방넘버?";

        playerCountText.text = $"{roomInfo.PlayerCount}";
        MaxPlayersText.text = $"/ {roomInfo.MaxPlayers}";
                
        mapNameText.text = roomInfo.CustomProperties.ContainsKey("Map")
            ? (string)roomInfo.CustomProperties["Map"]
            : "기본 맵";
        
        // 맵 이미지 설정
        var mapSprite = Resources.Load<Sprite>($"Maps/{mapNameText.text}");
        mapImage.sprite = mapSprite != null ? mapSprite : Resources.Load<Sprite>("Maps/default");

        //패스워드가 있다면 패스워드 아이콘 생성
        if (IsPasswrod(roomInfo))
        {
            roomPasswordText.text = roomInfo.CustomProperties.ContainsKey("Password")
            ? (string)roomInfo.CustomProperties["Password"]
            : "";
        }
        SetLockIcon(IsPasswrod(roomInfo));
    }
    //룸이 게임을 시작했는지를 반환하는 메서드
    public bool IsGameStarted(RoomInfo roomInfo)
    {
        return roomInfo.CustomProperties.ContainsKey("IsGameStart") && 
            (bool)roomInfo.CustomProperties["IsGameStart"];
;
    }
    //룸에 사람이 가득 찼는지를 반환하는 메서드
    public bool IsRoomFull(RoomInfo roomInfo)
    {
        return roomInfo.PlayerCount >= roomInfo.MaxPlayers;
    }
    //룸에 패스워드가 있는지를 반환하는 메서드
    public bool IsPasswrod(RoomInfo roomInfo)
    {
        return roomInfo.CustomProperties.ContainsKey("Password") &&
            !string.IsNullOrEmpty(roomInfo.CustomProperties["Password"] as string); //비밀번호 확인
    }
   
    //룸에 패스워드가 있다면 패스워드 아이콘을 띄움
    public void SetLockIcon(bool isPassword)
    {
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(isPassword);
        }
    }
}


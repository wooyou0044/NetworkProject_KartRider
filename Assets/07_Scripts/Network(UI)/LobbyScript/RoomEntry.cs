using Photon.Realtime;
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
        //방 정보 업데이트
        roomNameText.text = roomInfo.CustomProperties.ContainsKey("RoomName")
            ? (string)roomInfo.CustomProperties["RoomName"]
            : "방이름?";
        
        roomNumberText.text = roomInfo.CustomProperties.ContainsKey("RoomNumber")
            ? (string)roomInfo.CustomProperties["RoomNumber"].ToString()
            : "방넘버?";

        playerCountText.text = $"{roomInfo.PlayerCount}";
        MaxPlayersText.text = $"/ {roomInfo.MaxPlayers}";
        
        mapNameText.text = roomInfo.CustomProperties.ContainsKey("MapName")
            ? (string)roomInfo.CustomProperties["MapName"]
            : "기본 맵";

        // 맵 이미지 설정
        var mapSprite = Resources.Load<Sprite>($"Maps/{mapNameText.text}");
        mapImage.sprite = mapSprite != null ? mapSprite : Resources.Load<Sprite>("Maps/default");
        //패스워드가 있다면 패스워드 아이콘 생성
        bool passwrod = roomInfo.CustomProperties.ContainsKey("Password") &&
               !string.IsNullOrEmpty(roomInfo.CustomProperties["Password"] as string); // 비밀번호 확인
        SetLockIcon(passwrod);
    }
    public bool IsGameStarted(RoomInfo roomInfo)
    {
        return roomInfo.IsOpen;
    }
    public bool IsRoomFull(RoomInfo roomInfo)
    {
        return roomInfo.PlayerCount >= roomInfo.MaxPlayers;
    }
    public bool IsPasswrod(RoomInfo roomInfo)
    {
        return roomInfo.CustomProperties.ContainsKey("Password") &&
                       !string.IsNullOrEmpty(roomInfo.CustomProperties["Password"] as string); ; // 비밀번호 확인
    }    
    public void SetLockIcon(bool isPassword)
    {
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(isPassword);
        }
    }
}


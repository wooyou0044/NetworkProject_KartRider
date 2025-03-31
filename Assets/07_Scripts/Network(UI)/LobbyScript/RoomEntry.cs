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
        //�� ���� ������Ʈ
        roomNameText.text = roomInfo.CustomProperties.ContainsKey("RoomName")
            ? (string)roomInfo.CustomProperties["RoomName"]
            : "���̸�?";
        
        roomNumberText.text = roomInfo.CustomProperties.ContainsKey("RoomNumber")
            ? (string)roomInfo.CustomProperties["RoomNumber"].ToString()
            : "��ѹ�?";

        playerCountText.text = $"{roomInfo.PlayerCount}";
        MaxPlayersText.text = $"/ {roomInfo.MaxPlayers}";
        
        mapNameText.text = roomInfo.CustomProperties.ContainsKey("MapName")
            ? (string)roomInfo.CustomProperties["MapName"]
            : "�⺻ ��";

        // �� �̹��� ����
        var mapSprite = Resources.Load<Sprite>($"Maps/{mapNameText.text}");
        mapImage.sprite = mapSprite != null ? mapSprite : Resources.Load<Sprite>("Maps/default");
        //�н����尡 �ִٸ� �н����� ������ ����
        bool passwrod = roomInfo.CustomProperties.ContainsKey("Password") &&
               !string.IsNullOrEmpty(roomInfo.CustomProperties["Password"] as string); // ��й�ȣ Ȯ��
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
                       !string.IsNullOrEmpty(roomInfo.CustomProperties["Password"] as string); ; // ��й�ȣ Ȯ��
    }    
    public void SetLockIcon(bool isPassword)
    {
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(isPassword);
        }
    }
}


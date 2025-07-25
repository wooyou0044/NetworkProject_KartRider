using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomEntry : MonoBehaviour
{
    public TMP_Text roomNumberText;
    public TMP_Text roomNameText;
    public TMP_Text roomPasswordText;
    public TMP_Text playerCountText;
    public TMP_Text MaxPlayersText;
    public TMP_Text mapNameText;
    public Image mapImage;
    public Image lockIcon;
    public GameObject roomNamePanel;

    /// <summary>
    /// �� ������ �κ�ȭ�鿡�� �����ϱ� ���� RoomEntry 
    /// ���� �����ϰ� �Ǹ� ���� ������ �κ� �ִ� ����鿡�� ������
    /// </summary>
    /// <param name="roomInfo">���濡�� ������ �ִ� ������ ������ �޾ƿ�</param>
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        //�� �̸� �޾Ƽ� ó��, �� �̸� ����Ʈ �� ���̸�?
        roomNameText.text = roomInfo.CustomProperties.ContainsKey("RoomName")
            ? (string)roomInfo.CustomProperties["RoomName"]
            : "���̸� ��ü ���� ������ ��ĭ�� ������";
        //�ߺ����� �ʴ� ���� ���� ���� �̰ɷ� ���ΰ���
        roomNumberText.text = roomInfo.CustomProperties.ContainsKey("RoomNumber")
            ? (string)roomInfo.CustomProperties["RoomNumber"].ToString()
            : "�� ���� ���ڸ� ������ �κ�޴��� ���� �����ϴ� QueueȮ��";

        //�濡 ������ �ο� ��
        playerCountText.text = $"{roomInfo.PlayerCount}";
        //�濡 �ִ� ���� ������ �ο� ��
        MaxPlayersText.text = $"/ {roomInfo.MaxPlayers}";
                
        //�� �̸�
        mapNameText.text = roomInfo.CustomProperties.ContainsKey("Map")
            ? (string)roomInfo.CustomProperties["Map"]
            : "�� �̸� ���� ���ҽ����� Map Ȯ���ϱ�";
        
        //�� �̹��� ����
        //���� �� Maps Ȯ���ϱ�
        var mapSprite = Resources.Load<Sprite>($"Maps/{mapNameText.text}");
        mapImage.sprite = mapSprite != null ? mapSprite : Resources.Load<Sprite>($"Maps/{mapNameText.text}");

        //�н����尡 �ִٸ� ��й�
        if (IsPasswrod(roomInfo))
        {
            roomPasswordText.text = roomInfo.CustomProperties.ContainsKey("Password")
            ? (string)roomInfo.CustomProperties["Password"]
            : "";
        }
        //��й��� ��� �ڹ��� �������� ���
        SetLockIcon(IsPasswrod(roomInfo));
    }
    //���� ������ �����ߴ����� ��ȯ�ϴ� �޼���
    public bool IsGameStarted(RoomInfo roomInfo)
    {
        return roomInfo.CustomProperties.ContainsKey("IsGameStart") && 
            (bool)roomInfo.CustomProperties["IsGameStart"];
;
    }
    //�뿡 ����� ���� á������ ��ȯ�ϴ� �޼���
    public bool IsRoomFull(RoomInfo roomInfo)
    {
        return roomInfo.PlayerCount >= roomInfo.MaxPlayers;
    }
    //�뿡 �н����尡 �ִ����� ��ȯ�ϴ� �޼���
    public bool IsPasswrod(RoomInfo roomInfo)
    {
        return roomInfo.CustomProperties.ContainsKey("Password") &&
            !string.IsNullOrEmpty(roomInfo.CustomProperties["Password"] as string); //��й�ȣ Ȯ��
    }
   
    //�뿡 �н����尡 �ִٸ� �н����� �������� ���
    public void SetLockIcon(bool isPassword)
    {
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(isPassword);
        }
    }
}


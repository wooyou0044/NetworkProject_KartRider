using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;


public class PlayerInfo : MonoBehaviour
{
    [Header("ÇÃ·¹ÀÌ¾î ½½·Ô ÆÇ³Ú")]
    [SerializeField] public GameObject playerSlotPanel;
    [SerializeField] public GameObject playerInfoPanel;
    [SerializeField] public Image playerImg;
    [SerializeField] public TMP_Text playerNameText;
    [SerializeField] public Image playerIcon;

    public RoomManager roomManager;
    public void SetPlayerInfo(Player player)
    {
        playerNameText.text = player.NickName;
    }
}

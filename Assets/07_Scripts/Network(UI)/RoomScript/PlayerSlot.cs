using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// 플레이어를 자식으로 가지고 있을 룸에 배치된 슬롯 오브젝트
/// </summary>
public class PlayerSlot : MonoBehaviour
{
    [SerializeField] public PlayerPanel playerPanel;
    public int actorNumber;
    public bool isReady = false;
}

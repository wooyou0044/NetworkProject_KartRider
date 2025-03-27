using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSlot : MonoBehaviour
{
    [SerializeField] public PlayerPanel playerPanel;
    public int actorNumber;
    public string playerName;
    public bool IsEmpty => playerPanel == null; // playerPanel�� null�̸� true ��ȯ

    
}

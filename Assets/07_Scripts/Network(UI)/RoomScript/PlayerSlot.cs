using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �÷��̾ �ڽ����� ������ ���� �뿡 ��ġ�� ���� ������Ʈ
/// </summary>
public class PlayerSlot : MonoBehaviour
{
    [SerializeField] public PlayerPanel playerPanel;
    public int actorNumber;
    public bool isReady = false;
}

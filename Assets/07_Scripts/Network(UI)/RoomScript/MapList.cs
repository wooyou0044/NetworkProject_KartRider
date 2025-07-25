using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// �ʽ�ũ��Ʈ�� ������ �ִ� �� ������Ʈ��(��¥ �ʿ�����Ʈ ���ҽ����� ����) ���� �� ��
/// Ʈ�� �ǳڿ� ������Ŵ
/// </summary>
public class MapList : MonoBehaviour
{
    public Map[] map;

    void Start()
    {
        map = Resources.LoadAll<Map>("Map");
        if (map == null)
        {
            Debug.Log("���� �����ϴ�. ���ҽ� ���� ���� Ȯ�����ּ���. Maps�� �ƴմϴ�.");
        }
        for (int i = 0; i < map.Length; i++)
        {
            Instantiate(map[i], transform);
            map[i].GetComponent<Map>();                      
        }
    }
}

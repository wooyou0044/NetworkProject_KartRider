using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 맵스크립트를 가지고 있는 맵 오브젝트를(진짜 맵오브젝트 리소스에서 관리) 생성 한 뒤
/// 트랙 판넬에 생성시킴
/// </summary>
public class MapList : MonoBehaviour
{
    public Map[] map;

    void Start()
    {
        map = Resources.LoadAll<Map>("Map");
        if (map == null)
        {
            Debug.Log("맵이 없습니다. 리소스 파일 맵을 확인해주세요. Maps가 아닙니다.");
        }
        for (int i = 0; i < map.Length; i++)
        {
            Instantiate(map[i], transform);
            map[i].GetComponent<Map>();                      
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

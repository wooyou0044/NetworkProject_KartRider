using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkidMarkManager : MonoBehaviour
{
    [SerializeField] int initialPoolSize = 30;
    [SerializeField] GameObject skidMarkPrefab;

    List<GameObject> skidMarkPool;

    void Start()
    {
        skidMarkPool = new List<GameObject>();
        for(int i=0; i<initialPoolSize; i++)
        {
            CreateNewSkidMark();
        }
    }

    void Update()
    {
        
    }

    GameObject GetInactiveSkidMark()
    {
        foreach(GameObject skidMark in skidMarkPool)
        {
            if (skidMark.activeInHierarchy == false)
            {
                return skidMark;
            }
        }
        return null;
    }

    GameObject CreateNewSkidMark()
    {
        GameObject newSkidMark = Instantiate(skidMarkPrefab);
        newSkidMark.SetActive(false);
        skidMarkPool.Add(newSkidMark);
        return newSkidMark;
    }
}

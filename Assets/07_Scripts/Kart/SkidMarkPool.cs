using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkidMarkPool
{
    Queue<GameObject> unusedPool;
    Queue<GameObject> usedPool;
    GameObject skidMarkPrefab;

    public SkidMarkPool(GameObject skidMarkPrefab, int poolSize, Transform parent)
    {
        this.skidMarkPrefab = skidMarkPrefab;
        unusedPool = new Queue<GameObject>();
        usedPool = new Queue<GameObject>();

        for(int i=0; i<poolSize; i++)
        {
            GameObject skidMark = Object.Instantiate(skidMarkPrefab, parent);
            skidMark.SetActive(false);
            unusedPool.Enqueue(skidMark);
        }
    }

    public GameObject GetSkidMark()
    {
        if(unusedPool.Count > 0)
        {
            return unusedPool.Dequeue();
        }

        return null;
    }

    public void ReturnSkidMark(GameObject skidMark)
    {
        //skidMark.SetActive(false);
        unusedPool.Enqueue(skidMark);
    }
}

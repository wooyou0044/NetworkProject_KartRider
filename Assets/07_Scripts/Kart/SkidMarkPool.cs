using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkidMarkPool
{
    Queue<GameObject> pool;
    GameObject skidMarkPrefab;

    public SkidMarkPool(GameObject skidMarkPrefab, int poolSize)
    {
        this.skidMarkPrefab = skidMarkPrefab;
        pool = new Queue<GameObject>();

        for(int i=0; i<poolSize; i++)
        {
            GameObject skidMark = Object.Instantiate(skidMarkPrefab);
            skidMark.SetActive(false);
            pool.Enqueue(skidMark);
        }
    }

    public GameObject GetSkidMark()
    {
        if(pool.Count > 0)
        {
            return pool.Dequeue();
        }

        return null;
    }

    public void ReturnSkidMark(GameObject skidMark)
    {
        skidMark.SetActive(false);
        pool.Enqueue(skidMark);
    }
}

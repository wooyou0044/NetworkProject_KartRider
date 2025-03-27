using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartBodyController : MonoBehaviour
{
    [SerializeField] GameObject[] lampTrail;
    [SerializeField] Transform[] boostEffTransform;
    [SerializeField] GameObject boostEffect;

    void Start()
    {
        SetLampTrailActive(false);

        //foreach(Transform parentTrans in boostEffTransform)
        //{
        //    GameObject boost = Instantiate(boostEffect, parentTrans);
        //}

        //SetBoostEffectActive(false);
    }

    void Update()
    {
        
    }

    public void SetLampTrailActive(bool isActive)
    {
        foreach(GameObject lampObject in lampTrail)
        {
            lampObject.GetComponent<TrailRenderer>().enabled = isActive;
        }
    }

    public void SetBoostEffectActive(bool isActive)
    {
        if (boostEffTransform[0].childCount == 0)
        {
            foreach (Transform parentTrans in boostEffTransform)
            {
                GameObject boost = Instantiate(boostEffect, parentTrans);
            }
        }
        foreach(Transform transform in boostEffTransform)
        {
            transform.GetChild(0).gameObject.SetActive(isActive);
        }
    }
}

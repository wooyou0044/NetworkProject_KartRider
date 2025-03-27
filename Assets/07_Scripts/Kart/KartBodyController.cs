using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartBodyController : MonoBehaviour
{
    [SerializeField] GameObject[] lampTrail;
    //[SerializeField] Transform[] boostEffTransform;
    //[SerializeField] GameObject boostEffect;
    [SerializeField] GameObject[] boostEffect;

    void Start()
    {
        SetLampTrailActive(false);

        //foreach(Transform parentTrans in boostEffTransform)
        //{
        //    //Instantiate(boostEffect, parentTrans);
        //    //boost.transform.eulerAngles += new Vector3(0, 0,-90); 
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
        //foreach(Transform transform in boostEffTransform)
        //{
        //    transform.GetChild(0).gameObject.SetActive(isActive);
        //}

        foreach (GameObject boost in boostEffect)
        {
            boost.SetActive(isActive);
        }
    }
}

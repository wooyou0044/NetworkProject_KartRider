using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MapTriggerEventScript : MonoBehaviour
{
    private string _tag;

    void Awake()
    {
    
        _tag = gameObject.tag;        
    }

    public void OnTriggerEnter(Collider other)
    {

    }
}

using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class MapTriggerEventScript : MonoBehaviour
{
    private Collider _collider;
    private string _tag;
    
    public UnityEvent<Collider> collisionEvent;
    
    void Awake()
    {
        _collider = GetComponent<Collider>();
        _tag = gameObject.tag;        
    }

    public void OnTriggerEnter(Collider other)
    {
        collisionEvent.Invoke(other);
    }
}

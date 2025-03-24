using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkidMark : MonoBehaviour
{
    [SerializeField] Material skidMat;

    [SerializeField] float skidWidth;
    [SerializeField] LayerMask groundLayer;

    List<Vector3> vertices;
    List<int> triangles;
    List<Vector2> uvs;

    Mesh skidMesh;
    MeshRenderer skidRenderer;
    MeshFilter skidFilter;

    Vector3 lastPos;
    Vector3 lastNormal;
    bool isFirstPoint;

    void Awake()
    {
        skidFilter = gameObject.AddComponent<MeshFilter>();
        skidRenderer = gameObject.AddComponent<MeshRenderer>();


    }

    void Start()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
    }

    void Update()
    {
        
    }
}

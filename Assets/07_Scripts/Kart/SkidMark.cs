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
        skidFilter = GetComponent<MeshFilter>();
        skidRenderer = GetComponent<MeshRenderer>();

        skidMesh = new Mesh();
        skidFilter.mesh = skidMesh;
        skidRenderer.material = skidMat;
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

    public void AddSkidMark(Vector3 wheelPos)
    {
        RaycastHit hit;
        if (!Physics.Raycast(wheelPos + Vector3.up * 0.2f, Vector3.down, out hit, 0.5f, groundLayer))
        {
            return;
        }

        Vector3 groundPos = hit.point;
        Vector3 normal = hit.normal;

        if(isFirstPoint)
        {
            lastPos = groundPos;
            lastNormal = normal;
            isFirstPoint = false;
            return;
        }

        Vector3 direction = (groundPos - lastPos).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, normal).normalized * skidWidth * 0.5f;

        Vector3 left = groundPos - perpendicular;
        Vector3 right = groundPos + perpendicular;

        int index = vertices.Count;
        vertices.Add(left);
        vertices.Add(right);

        if(index >= 2)
        {
            triangles.Add(index - 2);
            triangles.Add(index - 1);
            triangles.Add(index);

            triangles.Add(index - 1);
            triangles.Add(index + 1);
            triangles.Add(index);
        }

        float uvX = (vertices.Count / 2) % 2 == 0 ? 0 : 1;
        uvs.Add(new Vector2(uvX, 0));
        uvs.Add(new Vector2(uvX, 1));

        lastPos = groundPos;
        lastNormal = normal;

        UpdateMesh();
    }

    void UpdateMesh()
    {
        skidMesh.Clear();
        skidMesh.vertices = vertices.ToArray();
        skidMesh.triangles = triangles.ToArray();
        skidMesh.uv = uvs.ToArray();
        skidMesh.RecalculateNormals();
    }
}

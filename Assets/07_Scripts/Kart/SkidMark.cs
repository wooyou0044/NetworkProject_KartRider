using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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

    float maxDistance = 1.0f;

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

        if (isFirstPoint)
        {
            lastPos = groundPos;
            lastNormal = normal;
            isFirstPoint = false;
            return;
        }

        float maxDistance = 1.0f;
        if (Vector3.Distance(lastPos, groundPos) > maxDistance)
        {
            lastPos = groundPos;
            return;
        }

        Vector3 direction = (groundPos - lastPos).normalized;
        Vector3 perpendicular = Vector3.Cross(normal, direction).normalized * skidWidth * 0.5f;

        Vector3 leftCurrent = groundPos - perpendicular;
        Vector3 rightCurrent = groundPos + perpendicular;

        Vector3 leftLast = lastPos - perpendicular;
        Vector3 rightLast = lastPos + perpendicular;

        int index = vertices.Count;
        vertices.Add(leftLast);
        vertices.Add(rightLast);
        vertices.Add(leftCurrent);
        vertices.Add(rightCurrent);

        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 1);

        triangles.Add(index + 1);
        triangles.Add(index + 2);
        triangles.Add(index + 3);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        lastPos = groundPos;
        lastNormal = normal;

        if (vertices.Count > 4)
        {
            UpdateMesh();
        }
    }

    void UpdateMesh()
    {
        skidMesh.Clear();
        skidMesh.vertices = vertices.ToArray();
        skidMesh.triangles = triangles.ToArray();
        skidMesh.uv = uvs.ToArray();
        skidMesh.RecalculateNormals();
    }

    public void ResetSkidMarks()
    {
        if(vertices == null)
        {
            vertices = new List<Vector3>();
        }
        if(triangles == null)
        {
            triangles = new List<int>();
        }
        if(uvs == null)
        {
            uvs = new List<Vector2>();
        }
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        skidMesh.Clear();
        skidMesh.vertices = vertices.ToArray();
        skidMesh.triangles = triangles.ToArray();
        skidMesh.uv = uvs.ToArray();
        skidMesh.RecalculateNormals();

        isFirstPoint = true;
    }
}

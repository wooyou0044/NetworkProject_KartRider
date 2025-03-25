using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class SkidMark : MonoBehaviour
{
    [SerializeField] Material skidMat;

    [SerializeField] float skidWidth;
    [SerializeField] LayerMask groundLayer;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

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

        isFirstPoint = true;
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

        float segmentDistance = 0.2f;

        float distance = Vector3.Distance(lastPos, groundPos);

        if (distance > 1f)
        {
            int steps = Mathf.CeilToInt(distance / segmentDistance);
            for (int i = 1; i <= steps; i++)
            {
                Vector3 interpolatedPos = Vector3.Lerp(lastPos, groundPos, (float)i / steps);
                CreateSkidMarkAt(interpolatedPos, normal);
            }
        }
        else if(distance >= segmentDistance)
        {
            CreateSkidMarkAt(groundPos, normal);
        }

        lastPos = groundPos;
        lastNormal = normal;
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
        lastPos = Vector3.zero;
    }

    void CreateSkidMarkAt(Vector3 position, Vector3 normal)
    {
        Vector3 direction = (position - lastPos).normalized;
        Vector3 perpendicular = Vector3.Cross(normal, direction).normalized * skidWidth * 0.5f;

        Vector3 leftCurrent = position - perpendicular;
        Vector3 rightCurrent = position + perpendicular;

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

        if (vertices.Count > 4)
        {
            UpdateMesh();
        }
    }
}

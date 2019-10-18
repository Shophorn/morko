using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BY SEBASTIAN LAGUE
/// </summary>
public class FieldOfView : MonoBehaviour
{
    [Range(-1f,1f)]
    public float hitOffset;

    public float viewRadius;
    [Range(0,360)]
    public float viewAngle;

    public float maskCutDistance = 0.2f;

    public MeshFilter viewMeshFilter;
    public MeshFilter fullMeshFilter;
    Mesh viewMesh;
    Mesh fullMesh;

    private void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        fullMesh = new Mesh();
        fullMesh.name = "Full Mesh";
        fullMeshFilter.mesh = fullMesh;
    }

    public Vector3 DirFromAngle(float degreeAngle, bool globalAngle)
    {
        if (globalAngle == false)
            degreeAngle += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(degreeAngle * Mathf.Deg2Rad), 0, Mathf.Cos(degreeAngle * Mathf.Deg2Rad));
    }

    public float meshResolution;

    private void LateUpdate()
    {
        DrawFieldOfView();
        DrawFullCone();
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            viewPoints.Add(newViewCast.point);
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i] + Vector3.forward * maskCutDistance);

            if(i<vertexCount-2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            
        }
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }
    void DrawFullCone()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = new ViewCastInfo(false, transform.position + DirFromAngle(angle, true) * viewRadius, viewRadius, angle);
            viewPoints.Add(newViewCast.point);
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i] + Vector3.forward * maskCutDistance);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

        }
        fullMesh.Clear();
        fullMesh.vertices = vertices;
        fullMesh.triangles = triangles;
        fullMesh.RecalculateNormals();
    }


    int mask = 1 << 8;
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if(Physics.Raycast(transform.position,dir,out hit, viewRadius,mask))
        {
            float distance = hit.distance + hitOffset;
            return new ViewCastInfo(true, transform.position + dir * distance, distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, hit.distance, globalAngle);
        }
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _distance, float _angle)
        {
            hit = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }
}

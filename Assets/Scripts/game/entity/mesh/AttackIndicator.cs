using System;
using UnityEngine;

public enum FovType
{
    Cone,
    Line,
    Projectile
}

public class AttackIndicator : MonoBehaviour
{
    public FovType type;
    public Vector3 origin;
    public Vector3 targetPosition;
    
    [Tooltip("Max attack distance.")]
    public float viewDistance = 200f;
    
    [Tooltip("Main arg to control indicator.\n* If type is Cone then it defines the angle of sector.\n* If type is Line then it defines the width of line.\n* If type is Projectile then it defines the attack radius of projectile.")]
    public float arg = 1;
   
    [Tooltip("If attack indicator can be blocked by collider in certain layer (Set it to false for penetrable attacks).")]
    public bool blockDetection = true;

    private Vector3 _direction;

    [SerializeField] private LayerMask mask;
    private MeshFilter _meshFilter;

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void LateUpdate()
    {
        Vector3[] vertices = null;
        Vector2[] uv = null;
        int[] triangles = null;

        if (type == FovType.Projectile)
        {
            int rayCount = 360;
            float minSplitDist = 0.1f;

            Vector3 position = targetPosition;
            float distance = (position - origin).magnitude;
            if (distance > viewDistance)
            {
                position = origin + (position - origin).normalized * viewDistance;
                distance = viewDistance;
            }

            int maxSplit = (int)Mathf.Ceil(distance / minSplitDist);

            vertices = new Vector3[2 * (maxSplit + 1) + rayCount + 1];
            uv = new Vector2[2 * (maxSplit + 1) + rayCount + 1];
            triangles = new int[6 * maxSplit + rayCount * 3];

            _projectileMesh(vertices, triangles, distance, minSplitDist);

            vertices[2 * (maxSplit + 1)] = position;

            int vertexIndex = 2 * (maxSplit + 1) + 1;
            int triangleIndex = 6 * maxSplit;
            for (int i = 0; i < 360; i += 360 / rayCount)
            {
                Vector3 rayDirection = new Vector3(Mathf.Cos(i * Mathf.PI / 180), 0, Mathf.Sin(i * Mathf.PI / 180));
                if (blockDetection)
                {
                    Physics.Raycast(position, rayDirection, out RaycastHit hit, arg, mask);
                    vertices[vertexIndex] = hit.collider ? hit.point : position + rayDirection * arg;
                }
                else
                {
                    vertices[vertexIndex] = position + rayDirection * arg;
                }
                
                if (i != 0)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex - 1;
                    triangles[triangleIndex + 2] = 2 * (maxSplit + 1);

                    triangleIndex += 3;
                }

                vertexIndex++;
            }

            triangles[triangleIndex] = 2 * (maxSplit + 1) + 1;
            triangles[triangleIndex + 1] = vertexIndex - 1;
            triangles[triangleIndex + 2] = 2 * (maxSplit + 1);
        }

        if (type == FovType.Cone)
        {
            _direction = Vector3.RotateTowards(_direction,
                (targetPosition - origin).normalized, 320 * Time.deltaTime, Mathf.PI * Time.deltaTime);
            int rayCount = (int)Math.Ceiling(arg);
            float angle = Mathf.Atan2(_direction.z, _direction.x) / Mathf.PI * 180 - arg / 2;
            float angleIncrease = arg / rayCount;

            vertices = new Vector3[rayCount + 2];
            uv = new Vector2[rayCount + 2];
            triangles = new int[rayCount * 3];

            vertices[0] = origin;

            int vertexIndex = 1;
            int triangleIndex = 0;
            for (int i = 0; i <= rayCount; i++)
            {
                Vector3 rayDirection =
                    new Vector3(Mathf.Cos(angle * Mathf.PI / 180), 0, Mathf.Sin(angle * Mathf.PI / 180));

                if (blockDetection)
                {
                    Physics.Raycast(origin, rayDirection, out RaycastHit hit, viewDistance, mask);
                    vertices[vertexIndex] = hit.collider ? hit.point : origin + rayDirection * viewDistance;
                }
                else
                {
                    vertices[vertexIndex] = origin + rayDirection * viewDistance;
                }

                if (i > 0)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex - 1;
                    triangles[triangleIndex + 2] = 0;

                    triangleIndex += 3;
                }

                vertexIndex++;
                angle += angleIncrease;
            }
        }
        else if (type == FovType.Line)
        {
            _direction = Vector3.RotateTowards(_direction,
                (targetPosition - origin).normalized, 320 * Time.deltaTime, Mathf.PI * Time.deltaTime);
            vertices = new Vector3[4];
            uv = new Vector2[4];
            triangles = new int[6];

            float len = arg / 2;
            Vector3 dir = new Vector3(_direction.x, 0, _direction.z).normalized;

            float distance;
            if (blockDetection)
            {
                Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance, mask);
                distance = hit.collider ? (hit.point - origin).magnitude : viewDistance;
            }
            else
            {
                distance = viewDistance;
            }

            Vector3 normal = new Vector3(len * dir.z, 0, len * -dir.x);
            vertices[0] = origin + normal;
            vertices[1] = origin - normal;
            vertices[2] = origin + normal + dir * distance;
            vertices[3] = origin - normal + dir * distance;

            triangles[0] = 3;
            triangles[1] = 0;
            triangles[2] = 1;
            triangles[3] = 3;
            triangles[4] = 2;
            triangles[5] = 0;
        }

        Mesh mesh = new();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        _meshFilter.mesh = mesh;
    }

    // Draw a projectile line indicator using y = x ^2
    private void _projectileMesh(Vector3[] vertices, int[] triangles, float distance, float minSplitDist)
    {
        float projectileWidth = 0.1f;
        float projectileMaxHeight = 5f;

        Vector3 direction = new Vector3(targetPosition.x - origin.x, 0, targetPosition.z - origin.z).normalized;
        Vector3 normal = new Vector3(projectileWidth * direction.z, 0, projectileWidth * -direction.x);

        int maxSplit = (int)Mathf.Ceil(distance / minSplitDist);

        float step = 0;
        int triangleIndex = 0;
        for (int i = 0; i <= maxSplit; i++)
        {
            Vector3 center = origin + new Vector3(direction.x * step * distance, projectileMaxHeight * (-Mathf.Pow(2 * step - 1, 2) + 1),
                direction.z * step * distance);
            vertices[2 * i] = center + normal;
            vertices[2 * i + 1] = center - normal;

            if (i != 0)
            {
                int prevPos = 2 * (i - 1);
                triangles[triangleIndex + 0] = prevPos + 3;
                triangles[triangleIndex + 1] = prevPos + 0;
                triangles[triangleIndex + 2] = prevPos + 1;
                triangles[triangleIndex + 3] = prevPos + 3;
                triangles[triangleIndex + 4] = prevPos + 2;
                triangles[triangleIndex + 5] = prevPos + 0;
                
                triangleIndex += 6;
            }
            step += minSplitDist;
        }
    }
}
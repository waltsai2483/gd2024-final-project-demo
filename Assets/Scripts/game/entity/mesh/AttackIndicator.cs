using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public enum FovType
{
    Cone,
    Line,
    Projectile
}

public class AttackIndicator : MonoBehaviour
{
    [Tooltip(
        "Type of attack.\n* Cone: A circular sector like shotgun.\n* Line: A straight line attack.\nProjectile: A projectile attack to certain position.")]
    public FovType type;

    [Tooltip("Position of attacker.")] public Vector3 origin;

    [Tooltip("Position of target (Mostly mouse position).")]
    public Vector3 targetPosition;

    [Tooltip("Max attack distance.")] public float viewDistance = 200f;

    [Tooltip(
        "Main arg to control indicator.\n* If type is Cone then it defines the angle of sector.\n* If type is Line then it defines the width of line.\n* If type is Projectile then it defines the attack radius of projectile.")]
    public float arg = 1;

    [Tooltip(
        "If attack indicator can be blocked by collider in certain layer (Set it to false for penetrable attacks).")]
    public bool blockDetection = true;

    private Vector3 _direction;

    [Tooltip("Layers where attack indicator can be blocked.")] [SerializeField]
    private LayerMask mask;

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
            float minSplitDist = 0.05f;

            Vector3 position = targetPosition;
            Vector2 xzVect = new Vector2(position.x - origin.x, position.z - origin.z);
            float distance = xzVect.magnitude;
            if (distance > viewDistance)
            {
                position = new Vector3(origin.x + xzVect.normalized.x * viewDistance, position.y, origin.z + xzVect.normalized.y * viewDistance);
                distance = viewDistance;
            }

            int maxSplit = (int)Mathf.Floor(distance / minSplitDist);

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
        else
        {
            _direction = Vector3.RotateTowards(_direction,
                (targetPosition - origin).normalized, 320 * Time.deltaTime, Mathf.PI * Time.deltaTime);
            if (type == FovType.Cone)
            {
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
                vertices = new Vector3[4];
                uv = new Vector2[4];
                triangles = new int[6];

                float len = arg / 2;
                Vector3 dir = new Vector3(_direction.x, _direction.y, _direction.z).normalized;

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
        float projectileWidth = 0.05f;

        Vector3 direction = new Vector3(targetPosition.x - origin.x, 0, targetPosition.z - origin.z).normalized;
        Vector3 normal = new Vector3(projectileWidth * direction.z, 0, projectileWidth * -direction.x);

        int maxSplit = (int)Mathf.Floor(distance / minSplitDist);

        float step = 0;
        float yDist = targetPosition.y - origin.y;
        float xDist = (1 + Mathf.Sqrt(1 + Mathf.Abs(yDist))) / 2;
        
        float yOffset = Mathf.Max(yDist, 0);
        int triangleIndex = 0;
        for (int i = 0; i <= maxSplit; i++)
        {
            float normalized = step / distance;

            float xOffset = yDist > 0 ? 1 - normalized : normalized;
            float operand = 2 * xOffset * xDist - 1;
            
            float y = -operand * operand + 1 + yOffset;
            Vector3 center = origin + new Vector3(direction.x * step, y, direction.z * step);
            
            vertices[2 * i] = center + normal;
            vertices[2 * i + 1] = center - normal;

            if (i != 0)
            {
                int prevVertex = 2 * (i - 1);
                triangles[triangleIndex + 0] = prevVertex + 3;
                triangles[triangleIndex + 1] = prevVertex + 0;
                triangles[triangleIndex + 2] = prevVertex + 1;
                triangles[triangleIndex + 3] = prevVertex + 3;
                triangles[triangleIndex + 4] = prevVertex + 2;
                triangles[triangleIndex + 5] = prevVertex + 0;

                triangleIndex += 6;
            }

            step += minSplitDist;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareTester : MonoBehaviour
{
    Vector2 topRight;
    Vector2 bottomRight;
    Vector2 bottomLeft;
    Vector2 topLeft;

    Vector2 topCenter;
    Vector2 rightCenter;
    Vector2 bottomCenter;
    Vector2 leftCenter;
    

    [Header(" Elements ")]
    [SerializeField] private MeshFilter filter;

    [Header(" Settings ")]
    [SerializeField] private float gridScale;

    // Start is called before the first frame update
    void Start()
    {
        topRight = gridScale * Vector2.one / 2;
        bottomRight = topRight + Vector2.down * gridScale;
        bottomLeft = bottomRight + Vector2.left * gridScale;
        topLeft = bottomLeft + Vector2.up * gridScale;

        topCenter = topRight + Vector2.left * gridScale / 2;
        rightCenter = bottomRight + Vector2.up * gridScale / 2;
        bottomCenter = bottomLeft + Vector2.right * gridScale / 2;
        leftCenter = topLeft + Vector2.down * gridScale / 2;


        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[6];

        vertices[0] = topCenter;
        vertices[1] = rightCenter;
        vertices[2] = bottomCenter;
        vertices[3] = leftCenter;

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        filter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(topRight, gridScale / 4f);
        Gizmos.DrawSphere(bottomLeft, gridScale / 4f);
        Gizmos.DrawSphere(bottomRight, gridScale / 4f);
        Gizmos.DrawSphere(topLeft, gridScale / 4f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(topCenter, gridScale / 8f);
        Gizmos.DrawSphere(rightCenter, gridScale / 8f);
        Gizmos.DrawSphere(bottomCenter, gridScale / 8f);
        Gizmos.DrawSphere(leftCenter, gridScale / 8f);
    }
}

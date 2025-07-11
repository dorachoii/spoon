using System;
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
    [SerializeField] private float isoValue;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    [Header(" Configuration ")]
    [SerializeField] private float topRightValue;
    [SerializeField] private float bottomRightValue;
    [SerializeField] private float bottomLeftValue;
    [SerializeField] private float topLeftValue;

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
    }

    // Update is called once per frame
    void Update()
    {
         Mesh mesh = new Mesh();

        vertices.Clear();
        triangles.Clear();

        Interporate();

        Triangulate(GetConfiguration());

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        filter.mesh = mesh;
    }

    private int GetConfiguration()
    {
        int configuration = 0;

        // 비트 연산자(<<, >>, |) 활용 가능
        //configuration = configuration | (1 << 0);   //0001
        if (topRightValue > isoValue) configuration += 1;
        if (bottomRightValue > isoValue) configuration += 2;
        if (bottomLeftValue > isoValue) configuration += 4;
        
        if (topLeftValue > isoValue) configuration += 8;

        return configuration;
    }

    private void Interporate()
    {
        // Top Center
        // float topLerp = (isoValue - topLeftValue) / (topRightValue - topLeftValue);
        // topLerp = Mathf.Clamp01(topLerp);
        // topCenter = topLeft + (topRight - topLeft) * topLerp;

        float topLerp = Mathf.InverseLerp(topLeftValue, topRightValue, isoValue);
        topLerp = Mathf.Clamp01(topLerp);
        topCenter = topLeft + (topRight - topLeft) * topLerp;

        float rightLerp = Mathf.InverseLerp(topRightValue, bottomRightValue, isoValue);
        rightLerp = Mathf.Clamp01(rightLerp);
        rightCenter = topRight + (bottomRight - topRight) * rightLerp;

        float bottomLerp = Mathf.InverseLerp(bottomLeftValue, bottomRightValue, isoValue);
        bottomLerp= Mathf.Clamp01(bottomLerp);
        bottomCenter = bottomLeft + (bottomRight - bottomLeft) * bottomLerp;

        float leftLerp = Mathf.InverseLerp(topLeftValue, bottomLeftValue, isoValue);
        leftLerp = Mathf.Clamp01(leftLerp);
        leftCenter = topLeft + (bottomLeft - topLeft) * leftLerp;
    }

    private void Triangulate(int configuration)
    {
        switch (configuration)
        {
            case 0:
                break;
            case 1:
                vertices.AddRange(new Vector3[] { topRight, rightCenter, topCenter });
                triangles.AddRange(new int[] { 0, 1, 2 });
                break;
            case 2:
                vertices.AddRange(new Vector3[] { rightCenter, bottomRight, bottomCenter });
                triangles.AddRange(new int[] { 0, 1, 2 });
                break;
            case 3:
                vertices.AddRange(new Vector3[] { topRight, bottomRight, bottomCenter, topCenter });
                triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3 });
                break;
            case 4:
                vertices.AddRange(new Vector3[] { bottomCenter, bottomLeft, leftCenter });
                triangles.AddRange(new int[] { 0, 1, 2 });
                break;
            case 5:
                vertices.AddRange(new Vector3[] { topRight, rightCenter, bottomCenter, bottomLeft, leftCenter, topCenter });
                triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5 });
                break;
            case 6:
                vertices.AddRange(new Vector3[] { bottomRight, bottomLeft, leftCenter, rightCenter });
                triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3 });
                break;
            case 7:
                vertices.AddRange(new Vector3[] { topRight, bottomRight, bottomLeft, leftCenter, topCenter });
                triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3, 0, 3, 4 });
                break;
            case 8:
                vertices.AddRange(new Vector3[] { leftCenter, topLeft, topCenter });
                triangles.AddRange(new int[] { 0, 1, 2 });
                break;
            case 9:
                vertices.AddRange(new Vector3[] { topRight, rightCenter, leftCenter, topLeft });
                triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3 });
                break;
            case 10:
                vertices.AddRange(new Vector3[] { rightCenter, bottomRight, bottomCenter, leftCenter, topLeft, topCenter });
                triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5 });
                break;
            case 11:
                vertices.AddRange(new Vector3[] { topRight, bottomRight, bottomCenter, leftCenter, topLeft });
                triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3, 0, 3, 4 });
                break;
            case 12:
                vertices.AddRange(new Vector3[] { bottomCenter, bottomLeft, topLeft, topCenter });
                triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3 });
                break;
            case 13:
                vertices.AddRange(new Vector3[] { topRight, rightCenter, bottomCenter, bottomLeft, topLeft });
                triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3, 0, 3, 4 });
                break;
            case 14:
                vertices.AddRange(new Vector3[] { rightCenter, bottomRight, bottomLeft, topLeft, topCenter });
                triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3, 0, 3, 4 });
                break;
            case 15:
                vertices.AddRange(new Vector3[] { topRight, bottomRight, bottomLeft, topLeft });
                triangles.AddRange(new int[] { 0, 1, 2, 0, 2, 3 });
                break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawSphere(topRight, gridScale / 8f);
        Gizmos.DrawSphere(bottomLeft, gridScale / 8f);
        Gizmos.DrawSphere(bottomRight, gridScale / 8f);
        Gizmos.DrawSphere(topLeft, gridScale / 8f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(topCenter, gridScale / 16f);
        Gizmos.DrawSphere(rightCenter, gridScale / 16f);
        Gizmos.DrawSphere(bottomCenter, gridScale / 16f);
        Gizmos.DrawSphere(leftCenter, gridScale / 16f);
    }
}

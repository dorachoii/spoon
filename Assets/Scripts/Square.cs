using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public struct Square
{
    private Vector2 position;

    private Vector2 topRight;
    private Vector2 bottomRight;
    private Vector2 bottomLeft;
    private Vector2 topLeft;

    private Vector2 topCenter;
    private Vector2 rightCenter;
    private Vector2 bottomCenter;
    private Vector2 leftCenter;

    private List<Vector3> vertices;
    private List<int> triangles;

    public Square(Vector2 position, float gridScale)
    {
        this.position = position;

        topRight = position + gridScale * Vector2.one / 2;
        bottomRight = topRight + Vector2.down * gridScale;
        bottomLeft = bottomRight + Vector2.left * gridScale;
        topLeft = bottomLeft + Vector2.up * gridScale;

        topCenter = topRight + Vector2.left * gridScale / 2;
        rightCenter = bottomRight + Vector2.up * gridScale / 2;
        bottomCenter = bottomLeft + Vector2.right * gridScale / 2;
        leftCenter = topLeft + Vector2.down * gridScale / 2;

        vertices = new List<Vector3>();
        triangles = new List<int>();
    }

    public void Triangulate(float isoValue, float[] values)
    {
        vertices.Clear();
        triangles.Clear();
        
        int configuration = GetConfiguration(isoValue, values);
        Interporate(isoValue, values);
        Triangulate(configuration);
    }

    private void Interporate(float isoValue, float[] values)
    {
        float topLerp = Mathf.InverseLerp(values[3], values[0], isoValue);
        topLerp = Mathf.Clamp01(topLerp);
        topCenter = topLeft + (topRight - topLeft) * topLerp;

        float rightLerp = Mathf.InverseLerp(values[0], values[1], isoValue);
        rightLerp = Mathf.Clamp01(rightLerp);
        rightCenter = topRight + (bottomRight - topRight) * rightLerp;

        float bottomLerp = Mathf.InverseLerp(values[2], values[1], isoValue);
        bottomLerp = Mathf.Clamp01(bottomLerp);
        bottomCenter = bottomLeft + (bottomRight - bottomLeft) * bottomLerp;

        float leftLerp = Mathf.InverseLerp(values[3], values[2], isoValue);
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

    private int GetConfiguration(float isoValue, float[] values)
    {
        int configuration = 0;

        // 비트 연산자(<<, >>, |) 활용 가능
        //configuration = configuration | (1 << 0);   //0001
        if (values[0] > isoValue) configuration += 1;
        if (values[1] > isoValue) configuration += 2;
        if (values[2] > isoValue) configuration += 4;
        if (values[3] > isoValue) configuration += 8;

        return configuration;
    }

    public Vector3[] GetVertices()
    {
        return vertices.ToArray();
    }

    public int[] GetTriangles()
    {
        return triangles.ToArray();
    }
}

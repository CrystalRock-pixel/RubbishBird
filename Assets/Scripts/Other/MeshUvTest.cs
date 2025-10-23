using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshUvTest : MonoBehaviour
{
    private List<Vector3> points = new List<Vector3>();
    void Start()
    {
        points.Add(new Vector3(0, 0, 0));
        points.Add(new Vector3(0, 3, 0));
        points.Add(new Vector3(3, 3, 0));
        points.Add(new Vector3(3, 0, 0));
        MeshDrawQuad();
    }
    void MeshDrawQuad()
    {
        //新建一个Mesh
        Mesh quadMesh = new Mesh();
        //把顶点的集合赋值给Mesh的顶点组
        quadMesh.vertices = points.ToArray();
        //设置quad顶点数量，画quad相当于画两个三角形
        int[] quadPoints = new int[6];
        quadPoints[0] = 0;
        quadPoints[1] = 1;
        quadPoints[2] = 3;
        quadPoints[3] = 3;
        quadPoints[4] = 1;
        quadPoints[5] = 2;
        //把Quad的数量（顺序）给Mesh的三角计数
        quadMesh.triangles = quadPoints;
        //添加uv的参数设置
        Vector2[] uvs = new Vector2[points.Count];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(0, 10);
        uvs[2] = new Vector2(10, 10);
        uvs[3] = new Vector2(10, 0);
        //把uvs信息赋值给QuadMesh.uv
        quadMesh.uv = uvs;
        GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex", new Vector2(1, 1));
        //设置quad的相关参数
        quadMesh.RecalculateBounds();
        quadMesh.RecalculateNormals();
        quadMesh.RecalculateTangents();
        //把设置Quad的相关参数的Mesh赋值给MeshFilter组件
        GetComponent<MeshFilter>().mesh = quadMesh;

    }
}

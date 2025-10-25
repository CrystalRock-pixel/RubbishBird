using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DirectionalShadowManager : MonoBehaviour
{
    [SerializeField] private Transform lightTransform;
    [SerializeField] private Transform planeTransform;
    [SerializeField] private List<GameObject> targetShadowObj;
    Dictionary<string,GameObject> shadowDic = new Dictionary<string,GameObject>();

    public float boxThickness;
    public float colliderOffset;

    private Vector3 lastLightPos= Vector3.zero; 
    private bool needRebuild = false;

    private void Start()
    {
        
    }

    private void Update()
    {
        if(lastLightPos!=lightTransform.position)
        {
            needRebuild = true;
            lastLightPos = lightTransform.position;
        }
        if (needRebuild)
        {
            //RebuildShadow();
            needRebuild = false;
        }
        RebuildShadow();
    }

    private void RebuildShadow()
    {
        bool isMesh = false;//为false时为Sprite
        foreach (var shadowObj in targetShadowObj)
        {
            //判断类型
            Vector3[] vertices;
            if (shadowObj.GetComponent<MeshFilter>() != null)
            {
                vertices = shadowObj.GetComponent<MeshFilter>().mesh.vertices;
                isMesh = true;
            }
            else if (shadowObj.GetComponent<SpriteRenderer>() != null)
            {
                SpriteRenderer spriteRenderer = shadowObj.GetComponent<SpriteRenderer>();
                // 获取转换矩阵：将本地坐标直接转换为世界坐标
                Matrix4x4 localToWorld = spriteRenderer.transform.localToWorldMatrix;

                Vector3[] vertices3D = Array.ConvertAll(shadowObj.GetComponent<SpriteRenderer>().sprite.vertices,v=>(Vector3)v);

                List<Vector3> normals = new List<Vector3>();
                for (int i = 0; i < vertices3D.Length; i++)
                {
                    Vector4 local4DVert = new Vector4(vertices3D[i].x, vertices3D[i].y, vertices3D[i].z, 1.0f);
                    Vector4 world4DVert = localToWorld * local4DVert;
                    Vector3 worldVert = new Vector3(world4DVert.x, world4DVert.y, world4DVert.z);
                    //normals.Add(spriteRenderer.transform.TransformPoint(vertices3D[i]));
                    normals.Add(worldVert);
                }
                vertices = normals.ToArray();
            }
            else return;

            // ****** 新增：获取定向光的投射方向 ******
            // 定向光的forward是它发出的方向，光线是沿着 -forward 传播的
            Vector3 lightDirection = -lightTransform.forward;

            // 获取平面法线
            Vector3 planeNormal = planeTransform.forward;
            Vector3 planePosition = planeTransform.position;


            // ****** 修改：求投影点 ******
            List<Vector3> shadowPoints = new List<Vector3>();
            foreach (var vertice_Local in vertices)
            {
                Vector3 vertice_World;
                if (isMesh)
                {
                    // 将Mesh的本地顶点转换为世界坐标
                    vertice_World = shadowObj.transform.TransformPoint(vertice_Local);
                }
                else
                {
                    // Sprite的顶点已经在前面的逻辑中转换到了世界坐标（vertice_Local就是vertice_World）
                    vertice_World = vertice_Local;
                }

                // 投影方向： 固定为lightDirection
                Vector3 rayDirection = lightDirection;

                // 计算 t 值（光线与平面交点）
                // t = ((planePosition - vertice_World) . planeNormal) / (rayDirection . planeNormal)
                float denominator = Vector3.Dot(rayDirection, planeNormal);

                // 防止分母接近零（光线平行于地面）
                if (Mathf.Abs(denominator) < 0.0001f)
                {
                    // 光线平行或几乎平行于地面，跳过此点
                    continue;
                }

                float numerator = Vector3.Dot((planePosition - vertice_World), planeNormal);
                float t = numerator / denominator;

                // 投影点： 起点是物体顶点，沿着光线方向延伸 t 距离
                Vector3 shadowPoint = vertice_World + rayDirection * t;

                shadowPoints.Add(shadowPoint);

                Debug.DrawLine(vertice_World, shadowPoint, Color.red); // 更改Debug绘制，现在是从物体顶点到影子点
            }
            //求凸包

            List<Vector3> hull = ConvexHull3D.ComputeConvexHullParallelToY(shadowPoints);
            for (int i = 0; i < hull.Count; i++)
            {
                Debug.DrawLine(hull[i], hull[(i + 1) % hull.Count], Color.green);

            }

            //构建Mesh
            var mesh = MeshGenerator.GenerateThickMesh(hull, boxThickness);

            //生成影子碰撞
            GameObject obj;
            if (!shadowDic.ContainsKey(shadowObj.name + "_showdow"))
            {
                obj = new GameObject(shadowObj.gameObject.name + "_showdow");
                obj.AddComponent<MeshCollider>().convex = true;
                obj.AddComponent<MeshFilter>();

                obj.transform.Translate(0, 0, colliderOffset);

                shadowDic.Add(shadowObj.name + "_showdow", obj);
            }
            else
            {
                obj = shadowDic[shadowObj.name + "_showdow"];
            }
            obj.GetComponent<MeshCollider>().sharedMesh = mesh;
            obj.GetComponent<MeshFilter>().mesh = mesh;

        }
    }
}

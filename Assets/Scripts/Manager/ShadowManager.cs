using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShadowManager : MonoBehaviour
{
    [SerializeField] private Transform lightTransform;
    [SerializeField] private Transform planeTransform;
    [SerializeField] private List<GameObject> targetShadowObj;
    Dictionary<string,GameObject> shadowDic = new Dictionary<string,GameObject>();

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
            //求投影点
            List<Vector3> shadowPoints = new List<Vector3>();
            foreach (var vertice in vertices)
            {
                Vector3 toVertice;
                if (isMesh)
                {
                    toVertice = (vertice + shadowObj.transform.position - lightTransform.position).normalized;
                }
                else
                {
                    toVertice = (vertice  - lightTransform.position).normalized;
                }

                float t = Vector3.Dot((planeTransform.position - lightTransform.position), planeTransform.forward) / Vector3.Dot(toVertice, planeTransform.forward);
                Vector3 shadowPoint = lightTransform.position + toVertice * t;

                shadowPoints.Add(shadowPoint);

                Debug.DrawLine(lightTransform.position, shadowPoint, Color.red);
            }
            foreach (var points in shadowPoints)
            {
                Debug.DrawLine(points, points + Vector3.up * 0.5f, Color.blue);
            }
            //求凸包

            List<Vector3> hull = ConvexHull3D.ComputeConvexHullParallelToY(shadowPoints);
            for (int i = 0; i < hull.Count; i++)
            {
                Debug.DrawLine(hull[i], hull[(i + 1) % hull.Count], Color.green);

            }

            //构建Mesh
            var mesh = MeshGenerator.GenerateThickMesh(hull, 1f);

            //生成影子碰撞
            GameObject obj;
            if (!shadowDic.ContainsKey(shadowObj.name + "_showdow"))
            {
                obj = new GameObject(shadowObj.gameObject.name + "_showdow");
                obj.AddComponent<MeshCollider>().convex = true;
                obj.AddComponent<MeshFilter>();

                obj.transform.Translate(0, 0, -0.5f);

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

public static class ConvexHull3D
{
    private const float Epsilon = 1e-6f;

    private struct IndexedVector2
    {
        public int originalIndex;
        public Vector2 v2;
    }

    /// <summary>
    /// 计算与 Y 轴平行的共面 Vector3 点集的凸包。
    /// 通过比较 X 和 Z 轴的范围，选择投影到 XY 或 YZ 平面。
    /// </summary>
    public static List<Vector3> ComputeConvexHullParallelToY(List<Vector3> points)
    {
        if (points == null || points.Count < 3)
        {
            return points;
        }

        // 1. 确定投影平面：计算 X 和 Z 轴的范围
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var p in points)
        {
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minZ = Mathf.Min(minZ, p.z);
            maxZ = Mathf.Max(maxZ, p.z);
        }

        float rangeX = maxX - minX;
        float rangeZ = maxZ - minZ;

        List<IndexedVector2> projectedPoints;
        bool isProjectingToXY = rangeX >= rangeZ; // 如果 X 范围更大或相等，投影到 XY 平面

        // 2. 降维到 2D
        if (isProjectingToXY)
        {
            // 投影到 XY 平面 (X轴 -> X', Y轴 -> Y')，忽略 Z 坐标
            projectedPoints = points
                .Select((p, i) => new IndexedVector2 { originalIndex = i, v2 = new Vector2(p.x, p.y) })
                .ToList();
            //Debug.Log("凸包计算：投影到 XY 平面 (忽略 Z 坐标)");
        }
        else
        {
            // 投影到 YZ 平面 (Z轴 -> X', Y轴 -> Y')，忽略 X 坐标
            projectedPoints = points
                .Select((p, i) => new IndexedVector2 { originalIndex = i, v2 = new Vector2(p.z, p.y) })
                .ToList();
            //Debug.Log("凸包计算：投影到 YZ 平面 (忽略 X 坐标)");
        }

        // 3. 计算 2D 凸包 (Monotone Chain)
        List<IndexedVector2> hull2D = MonotoneChain(projectedPoints);

        // 4. 结果回溯
        List<Vector3> hull3D = hull2D
            .Select(indexedPoint => points[indexedPoint.originalIndex])
            .ToList();

        return hull3D;
    }

    // 2D 叉积函数 (Monotone Chain 算法所需)
    private static float CrossProduct(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        return (p1.x - p0.x) * (p2.y - p0.y) - (p1.y - p0.y) * (p2.x - p0.x);
    }

    // Monotone Chain 凸包算法（与上次代码相同）
    private static List<IndexedVector2> MonotoneChain(List<IndexedVector2> points)
    {
        int n = points.Count;
        if (n <= 3) return points;

        // 1. 排序：按 X 坐标升序，若 X 相同则按 Y 坐标升序
        points.Sort((a, b) =>
        {
            int cmp = a.v2.x.CompareTo(b.v2.x);
            if (cmp != 0) return cmp;
            return a.v2.y.CompareTo(b.v2.y);
        });

        List<IndexedVector2> hull = new List<IndexedVector2>(n);

        // 2. 构建下凸包
        for (int i = 0; i < n; i++)
        {
            while (hull.Count >= 2 &&
                   CrossProduct(hull[hull.Count - 2].v2, hull[hull.Count - 1].v2, points[i].v2) <= Epsilon)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(points[i]);
        }

        int lowerHullSize = hull.Count;

        // 3. 构建上凸包
        for (int i = n - 2; i >= 0; i--)
        {
            while (hull.Count > lowerHullSize &&
                   CrossProduct(hull[hull.Count - 2].v2, hull[hull.Count - 1].v2, points[i].v2) <= Epsilon)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(points[i]);
        }

        // 移除重复的起点
        hull.RemoveAt(hull.Count - 1);

        return hull;
    }
}

public static class MeshGenerator
{
    /// <summary>
    /// 根据共面凸包点集，生成具有厚度的 Mesh。
    /// 假设 hullPoints 已按逆时针顺序排列。
    /// </summary>
    /// <param name="hullPoints">凸包点集（List<Vector3>，逆时针顺序）</param>
    /// <param name="thickness">Mesh 的厚度</param>
    /// <returns>生成的 Mesh 对象</returns>
    public static Mesh GenerateThickMesh(List<Vector3> hullPoints, float thickness)
    {
        if (hullPoints == null || hullPoints.Count < 3)
        {
            Debug.LogError("凸包顶点数量不足 3 个，无法生成 Mesh。");
            return null;
        }

        // --- 1. 计算平面法线 ---
        // 使用前三个点计算法线。凸包点通常不共线。
        Vector3 a = hullPoints[0];
        Vector3 b = hullPoints[1];
        Vector3 c = hullPoints[2];
        // 叉积得到法线（右手定则，如果 hullPoints 是逆时针，法线指向 "外侧"）
        Vector3 normal = Vector3.Cross(b - a, c - a).normalized;

        // 偏移向量：法线 * 厚度的一半
        Vector3 offset = normal * (thickness / 2f);

        // --- 2. 初始化顶点和三角形列表 ---
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        int N = hullPoints.Count; // 凸包顶点数量

        // --- 3. 生成正面 (Front Face) 和 背面 (Back Face) ---

        // 前面 (Front Face - 沿着法线正方向)
        int frontStartIndex = vertices.Count;
        for (int i = 0; i < N; i++)
        {
            vertices.Add(hullPoints[i] + offset);
        }

        // 三角剖分：由于是凸多边形，只需简单地以第一个顶点为扇心进行三角剖分
        // 顺序: (0, 1, 2), (0, 2, 3), (0, 3, 4), ...
        for (int i = 1; i < N - 1; i++)
        {
            triangles.Add(frontStartIndex);
            triangles.Add(frontStartIndex + i);
            triangles.Add(frontStartIndex + i + 1);
        }

        // 背面 (Back Face - 沿着法线负方向)
        int backStartIndex = vertices.Count;
        for (int i = 0; i < N; i++)
        {
            vertices.Add(hullPoints[i] - offset);
        }

        // 背面的三角剖分：顶点顺序必须与正面相反，以确保法线朝向 "外侧" (与正面法线方向相同)
        // 顺序: (N, N+2, N+1), (N, N+3, N+2), ...
        // 注意：这里的顶点是 hullPoints[i] - offset，它们已经按逆时针顺序排列，但我们希望法线朝里
        // 所以我们需要以顺时针顺序添加索引: (0, 1, 2) -> (0, 2, 1)
        for (int i = 1; i < N - 1; i++)
        {
            triangles.Add(backStartIndex);
            triangles.Add(backStartIndex + i + 1); // 交换 i 和 i+1 的顺序
            triangles.Add(backStartIndex + i);
        }

        // --- 4. 生成侧面 (Sides) ---
        // 每个侧面是一个矩形（两个三角形）
        // 侧面数量 = N
        for (int i = 0; i < N; i++)
        {
            // 当前边 i -> (i+1)%N
            int i_next = (i + 1) % N;

            // 侧面的 4 个顶点索引 (Front: f, Back: b)
            // f0 = frontStartIndex + i
            // f1 = frontStartIndex + i_next
            // b0 = backStartIndex + i
            // b1 = backStartIndex + i_next
            int f0 = frontStartIndex + i;
            int f1 = frontStartIndex + i_next;
            int b0 = backStartIndex + i;
            int b1 = backStartIndex + i_next;

            // 侧面三角形 1: (f0, f1, b0) - 逆时针
            triangles.Add(f0);
            triangles.Add(b0);
            triangles.Add(f1);

            // 侧面三角形 2: (f1, b0, b1) - 逆时针
            triangles.Add(f1);
            triangles.Add(b1);
            triangles.Add(b0);
        }

        // --- 5. 创建并返回 Mesh ---
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        // 自动重新计算法线，确保光照正确
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}

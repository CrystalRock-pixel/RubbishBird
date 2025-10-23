using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConsistentWorldUV : MonoBehaviour
{
    private Mesh mesh;
    private Vector2[] originalUVs;

    // 我们需要一个参考值，例如：贴图的一个单位在世界空间中应该占据 1.0 米。
    public float ReferenceWorldUnit = 1.0f;

    void Awake()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            // 确保我们操作的是 Mesh 实例，而不是共享资源
            mesh = meshFilter.mesh;

            // 存储原始的 UV 数据
            originalUVs = mesh.uv;

            // 初始化时应用一次
            ApplyConsistentUVs();
        }
    }

    public void ApplyConsistentUVs()
    {
        if (mesh == null || originalUVs == null || originalUVs.Length == 0) return;

        // 1. 获取物体的世界空间缩放
        Vector3 worldScale = transform.lossyScale;

        // **关键修正：假设墙面在 X-Y 平面**
        // U 轴 (横向平铺) 对应物体的 X 轴缩放
        float scaleU = worldScale.x / ReferenceWorldUnit;
        // V 轴 (纵向平铺) 对应物体的 Y 轴缩放
        float scaleV = worldScale.y / ReferenceWorldUnit;

        // 如果您的墙面是平铺在 X-Z 平面（例如地面），请使用：
        // float scaleV = worldScale.z / ReferenceWorldUnit; 

        // 2. 准备新的 UV 数组
        Vector2[] newUVs = new Vector2[originalUVs.Length];

        for (int i = 0; i < originalUVs.Length; i++)
        {
            // 直接将原始 UV 乘以世界缩放 (并除以参考单位)
            newUVs[i].x = originalUVs[i].x * scaleU;
            newUVs[i].y = originalUVs[i].y * scaleV;
        }

        // 3. 写入新的 UV 数组
        mesh.uv = newUVs;

        // 4. **强制材质 Tiling 为 (1, 1)** // 仅作为测试步骤，确保Shader没有对新的 UV 进行二次缩放
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material.HasProperty("_MainTex"))
        {
            renderer.material.mainTextureScale = new Vector2(1f, 1f);
        }
    }

    // 如果运行时有缩放变化，保持更新
    void LateUpdate()
    {
        ApplyConsistentUVs();
    }
    }

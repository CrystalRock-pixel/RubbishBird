using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentWall : MonoBehaviour
{
    // 公开字段：用于在Inspector中调整透明度
    private const string SURFACE_PROP = "_Surface";
    [Header("透明度设置")]
    [Tooltip("完全透明时的目标 Alpha 值 (0.0 = 完全透明)")]
    [Range(0f, 1f)]
    public float targetAlpha = 0.3f;

    [Tooltip("完全不透明时的目标 Alpha 值")]
    [Range(0f, 1f)]
    public float opaqueAlpha = 1.0f;

    [Tooltip("透明度过渡的速度")]
    public float fadeSpeed = 5f;

    // 内部私有字段
    private Renderer objRenderer;
    private Material objMaterial;
    private Color originalColor;
    private float currentAlpha;
    private bool isOccluded = false;

    private bool isTransparent = false;
    // 公开透明度属性
    public float CurrentTransparency => 1.0f - currentAlpha; // 透明度 (0.0=不透明, 1.0=完全透明)

    // 公开 Alpha 值属性
    public float CurrentAlpha
    {
        get => currentAlpha;
        private set => currentAlpha = value;
    }

    void Awake()
    {
        objRenderer = GetComponent<Renderer>();
        // 获取材质实例，避免修改原始材质
        objMaterial = objRenderer.material;

        //// 确保材质支持透明度
        //SetupMaterialForTransparency();
        originalColor = objMaterial.color;
        currentAlpha = originalColor.a;

        // 初始状态设置为不透明
        SetMaterialAlpha(opaqueAlpha);
        SetMaterialOpaque();
    }

    //void SetupMaterialForTransparency()
    //{
    //    // 关键步骤：为材质设置正确的渲染模式以支持透明度
    //    // 这通常需要将材质的渲染模式设置为 "Fade" 或 "Transparent"
    //    // 具体的设置取决于你的渲染管线（Built-in, URP, HDRP）和 Shader 类型。

    //    // 针对标准/内置渲染管线的简单示例 (可能需要根据实际材质调整)
    //    if (objMaterial.HasProperty("_Mode"))
    //    {
    //        objMaterial.SetInt("_Mode", 2); // 2 对应 Fade 模式
    //    }
    //    objMaterial.SetOverrideTag("RenderType", "Transparent");
    //    objMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    //    objMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    //    objMaterial.SetInt("_ZWrite", 0);
    //    objMaterial.DisableKeyword("_ALPHATEST_ON");
    //    objMaterial.EnableKeyword("_ALPHABLEND_ON");
    //    objMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    //    objMaterial.renderQueue = 3000;
    //}

    // 设置材质的 Alpha 值
    private void SetMaterialAlpha(float alpha)
    {
        Color newColor = originalColor;
        newColor.a = alpha;
        objMaterial.color = newColor;
        currentAlpha = alpha;
    }

    // 外部调用：设置透明化状态
    public void SetOcclusion(bool occluded)
    {
        isOccluded = occluded;
    }

    void Update()
    {
        float target = isOccluded ? targetAlpha : opaqueAlpha;
        float alphaDifference = Mathf.Abs(currentAlpha - target);

        if (alphaDifference > 0.001f)
        {
            if (isOccluded && !isTransparent)
            {
                // 切换到 Transparent 模式
                SetMaterialTransparent();
            }

            // 平滑过渡 Alpha 值
            float newAlpha = Mathf.MoveTowards(currentAlpha, target, fadeSpeed * Time.deltaTime);
            SetMaterialAlpha(newAlpha);
        }
        else // Alpha 值已达目标
        {
            if (!isOccluded && isTransparent)
            {
                // 过渡完成后，切换回 Opaque 模式
                SetMaterialAlpha(opaqueAlpha);
                SetMaterialOpaque();
            }
        }
    }

    // 在对象销毁时清理材质，避免内存泄漏
    private void OnDestroy()
    {
        if (objMaterial != null)
        {
            Destroy(objMaterial);
        }
    }

    public void SetMaterialTransparent()
    {
        isTransparent = true;

        // URP/HDRP 核心切换
        objMaterial.SetFloat(SURFACE_PROP, 1f); // _Surface = 1 (Transparent)
        objMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        // 强制渲染设置 (重要!)
        objMaterial.SetOverrideTag("RenderType", "Transparent");
        objMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        objMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        objMaterial.SetInt("_ZWrite", 0); // 关闭深度写入
        objMaterial.renderQueue = 3000; // 切换到透明队列

        // URP 混合模式属性（如果存在）
        if (objMaterial.HasProperty("_BlendMode"))
        {
            // 0 = Alpha (URP Lit Shader的Blend Mode)
            objMaterial.SetFloat("_BlendMode", 0f);
        }
    }
    public void SetMaterialOpaque()
    {
        isTransparent = false;

        // URP/HDRP 核心切换
        objMaterial.SetFloat(SURFACE_PROP, 0f); // _Surface = 0 (Opaque)
        objMaterial.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");

        // 强制渲染设置 (重要!)
        objMaterial.SetOverrideTag("RenderType", "Opaque");
        objMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One); // 禁用混合
        objMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero); // 禁用混合
        objMaterial.SetInt("_ZWrite", 1); // 开启深度写入
        objMaterial.renderQueue = -1; // 或 2000，-1 表示使用 Shader 默认队列 (通常是 2000)

        // URP 混合模式属性（如果存在）
        if (objMaterial.HasProperty("_BlendMode"))
        {
            // 1 = Opaque (URP Lit Shader的Blend Mode)
            objMaterial.SetFloat("_BlendMode", 1f);
        }
    }
}

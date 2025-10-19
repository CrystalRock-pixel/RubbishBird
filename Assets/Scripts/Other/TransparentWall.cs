using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentWall : MonoBehaviour
{
    // 公开字段：用于在Inspector中调整透明度
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

        if (Mathf.Abs(currentAlpha - target) > 0.001f)
        {
            // 平滑过渡 Alpha 值
            float newAlpha = Mathf.MoveTowards(currentAlpha, target, fadeSpeed * Time.deltaTime);
            SetMaterialAlpha(newAlpha);
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
}

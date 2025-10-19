using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

// 这段代码的作用是为 MeshRenderer 组件添加自定义的 Inspector 界面
[CustomEditor(typeof(MeshRenderer))]
public class MeshRendererSortingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制 MeshRenderer 默认的 Inspector 界面
        base.OnInspectorGUI();

        MeshRenderer renderer = target as MeshRenderer;

        // --- Sorting Layer Name ---
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();

        // 获取所有排序层并显示在下拉菜单中
        int newId = DrawSortingLayersPopup(renderer.sortingLayerID);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(renderer, "Change Sorting Layer");
            renderer.sortingLayerID = newId;
        }
        EditorGUILayout.EndHorizontal();

        // --- Order in Layer ---
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();

        // 显示 Order in Layer 的整数输入框
        int order = EditorGUILayout.IntField("Sorting Order", renderer.sortingOrder);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(renderer, "Change Sorting Order");
            renderer.sortingOrder = order;
        }
        EditorGUILayout.EndHorizontal();
    }

    // 辅助方法：绘制 Sorting Layer 的下拉菜单
    int DrawSortingLayersPopup(int layerID)
    {
        var layers = SortingLayer.layers;
        var names = layers.Select(l => l.name).ToArray();

        if (!SortingLayer.IsValid(layerID))
        {
            // 如果当前的 layerID 无效，则默认选中第一个
            layerID = layers[0].id;
        }

        // 获取当前 layerID 在 SortingLayer 列表中的索引值
        var layerValue = SortingLayer.GetLayerValueFromID(layerID);

        // 绘制下拉菜单并获取用户选择的新索引
        var newLayerValue = EditorGUILayout.Popup("Sorting Layer", layerValue, names);

        // 返回新索引对应的 SortingLayer 的 ID
        return layers[newLayerValue].id;
    }
}
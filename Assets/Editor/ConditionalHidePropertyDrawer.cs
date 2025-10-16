using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHidePropertyDrawer : PropertyDrawer
{
    private ConditionalHideAttribute conditionalAttribute
    {
        get { return (ConditionalHideAttribute)attribute; }
    }

    // 检查字段是否应该隐藏
    private bool IsHidden(SerializedProperty property)
    {
        // 查找条件字段（布尔值）
        string propertyPath = property.propertyPath; // 获取当前属性的路径
        string conditionPath = propertyPath.Replace(property.name, conditionalAttribute.ConditionalSourceField); // 用条件字段名替换当前字段名

        SerializedProperty sourceProperty = property.serializedObject.FindProperty(conditionPath);

        if (sourceProperty != null && sourceProperty.propertyType == SerializedPropertyType.Boolean)
        {
            bool conditionValue = sourceProperty.boolValue;

            // 根据 HideInInspector 的设置，确定是否应该隐藏
            // 如果 HideInInspector 为 true，则当 conditionValue 为 true 时隐藏
            // 如果 HideInInspector 为 false，则当 conditionValue 为 false 时隐藏
            return (conditionValue == conditionalAttribute.HideInInspector);
        }
        else
        {
            // 如果找不到布尔条件的字段，或者字段类型不正确，则显示警告并显示该字段
            Debug.LogWarning("ConditionalHide 特性需要一个有效的 bool 字段名: " + conditionalAttribute.ConditionalSourceField +
                             " in object: " + property.serializedObject.targetObject.name);
            return false;
        }
    }

    // 重写 GetPropertyHeight，用于确定属性占用的垂直空间
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (IsHidden(property))
        {
            // 如果隐藏，高度为 0，同时避免 Unity 绘制空白空间
            return -EditorGUIUtility.standardVerticalSpacing;
        }

        // 否则，返回默认高度
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    // 重写 OnGUI，用于绘制自定义属性
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 如果不隐藏，则绘制属性
        if (!IsHidden(property))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}

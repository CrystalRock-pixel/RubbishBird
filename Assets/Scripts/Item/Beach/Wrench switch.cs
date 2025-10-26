using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenchSwitch : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }
    public bool Disposable { get => true; set => throw new System.NotImplementedException(); }

    public bool isFixed = false;
    private Player player => Player.Instance;
    public GameObject towerLight;
    public bool Interact()
    {
        if (isFixed == false)
        {
            IInteractive interactiveItem = player.GetCurrentInteractiveItem();
            if (interactiveItem != null)
            {
                string name = interactiveItem.instance.name;
                if (name == "齿轮")
                {
                    isFixed = true;
                    player.OverInteractive();
                    Destroy(interactiveItem.instance.gameObject);
                    DialogManager.Instance.ShowDialog("开关修理成功", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                    return true;
                }
                else
                {
                    DialogManager.Instance.ShowDialog("这个可修不了开关", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                    return false;
                }
            }
            else
            {
                DialogManager.Instance.ShowDialog("需要一个齿轮来修理开关", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                return false;
            }
        }
        else
        {
            RotateObjectAroundWorldY(towerLight, 30f);
            DialogManager.Instance.ShowDialog("旋转灯塔", transform.position, new Vector3(0, 2.5f, 0), this.transform);
            return true;
        }
    }

    public void RotateObjectAroundWorldY(GameObject targetObject, float angleDegrees)
    {
        // 确保目标物体不为空
        if (targetObject == null)
        {
            Debug.LogError("目标物体 (targetObject) 为空，无法执行旋转。");
            return;
        }

        // 创建一个表示绕世界Y轴旋转的角度的四元数
        // Quaternion.Euler(x, y, z) 可以将欧拉角转换为四元数
        // 这里的 (0, angleDegrees, 0) 表示绕Y轴旋转angleDegrees度
        Quaternion rotation = Quaternion.Euler(0f, angleDegrees, 0f);

        // 使用 Transform.rotation 或 Transform.localRotation 来进行旋转：

        // 1. 使用 Transform.rotation (世界坐标系旋转)
        // targetObject.transform.rotation = rotation * targetObject.transform.rotation;

        // 2. 推荐使用 Transform.Rotate 方法，它更简洁且提供了 Space 选项
        // 使用 Space.World 确保旋转是相对于世界坐标系的
        targetObject.transform.Rotate(0f, angleDegrees, 0f, Space.World);

        // -----------------------------------------------------------
        // 替代方法：使用四元数相乘（更底层，但也是标准做法）
        // targetObject.transform.rotation = rotation * targetObject.transform.rotation;

        Debug.Log(targetObject.name + " 已绕世界Y轴旋转 " + angleDegrees + " 度。");
    }
    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }
}

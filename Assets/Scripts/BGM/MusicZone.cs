using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZone : MonoBehaviour
{
    [Header("该区域的背景音乐")]
    public AudioClip zoneMusic;

    [Header("触发器设置")]
    [Tooltip("防止频繁切换的冷却时间")]
    public float cooldownTime = 2f;

    [Header("优先级设置（嵌套场景用）")]
    [Tooltip("数字越大优先级越高，嵌套的小场景设置更高的优先级")]
    public int priority = 0;

    private float lastTriggerTime = -999f;

    void OnTriggerEnter(Collider other)
    {
        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            // 冷却时间检查，防止边界反复触发
            if (Time.time - lastTriggerTime > cooldownTime)
            {
                lastTriggerTime = Time.time;

                if (MusicManager.Instance != null && zoneMusic != null)
                {
                    MusicManager.Instance.EnterZone(this);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 玩家离开区域
        if (other.CompareTag("Player"))
        {
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.ExitZone(this);
            }
        }
    }
}

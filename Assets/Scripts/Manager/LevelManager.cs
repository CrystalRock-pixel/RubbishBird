using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

// 继承自 EventArgs，用于在事件中传递参数
public class AnimationCompleteEventArgs : EventArgs
{
    // 你需要传递给UI引导流程的参数，例如：
    public string newGoalText { get; private set; }
    public string newTabMenuContent { get; private set; }

    // 构造函数，用于在触发事件时设置参数
    public AnimationCompleteEventArgs(string _newGoalText, string _newTabPrompt)
    {
        this.newGoalText = _newGoalText;
        this.newTabMenuContent = _newTabPrompt;
    }
}

public class LevelManager : MonoBehaviour
{
    public enum LevelType
    {
        Coffee,
        Beach,
        Three,
    }
    private Player player;
    public LevelType type;

    private void Start()
    {
        player = Player.Instance;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")&&type==LevelType.Coffee)
        {
            player.canFeather = true;
            AnimationCompleteEventArgs args=new AnimationCompleteEventArgs(
                "按E拔羽毛",
                "目标:  修复灯塔，开灯寻找线索 \n 左键:  啄 \n 右键:  鸣叫 \n E键:  拔一根羽毛"
            );
            VideoManager.Instance.PlayVideoClip(VideoManager.Instance.electricity,args);

        }
    }

}

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
    public static LevelManager instance;
    public enum LevelType
    {
        Coffee,
        Beach,
        Three,
    }
    private Player player;
    public LevelType type;

    public GameObject trueCoffeeLab;
    public GameObject fakeCoffeeLab;

    public GameObject lightTower;

    public Transform level2Point;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        player = Player.Instance;
        lightTower.SetActive(false);
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

            trueCoffeeLab.transform.position += new Vector3(0, 50, 0);
            trueCoffeeLab.SetActive(false);
            fakeCoffeeLab.SetActive(true);

            player.transform.position = level2Point.position;

            VideoManager.Instance.PlayVideoClip(VideoManager.Instance.electricity,args);
        }
    }

    public void GotoLevelThree()
    {
        player.canJump = true;
        AnimationCompleteEventArgs args = new AnimationCompleteEventArgs(
              "按空格键跳跃",
              "目标:  利用刚拿到的脚，返回咖啡馆修复bug \n 左键:  啄 \n 右键:  鸣叫 \n E键:  拔一根羽毛"
          );
        lightTower.SetActive(true);
        VideoManager.Instance.PlayVideoClip(VideoManager.Instance.findFeet, args);
    }

}

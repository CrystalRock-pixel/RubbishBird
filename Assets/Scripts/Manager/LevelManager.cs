using System;
using Unity.VisualScripting;
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
    public GameObject persons;
    public GameObject parrot;
    public GameObject coffeeDoor;
    public GameObject fakeCoffeeDoor;

    public GameObject coffeeLight;
    public GameObject beachLight;

    public GameObject shadowObject;

    public RuntimeAnimatorController animatorController;

    public Transform level2Point;

    private int index = 0;

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
        persons.SetActive(false);
        parrot.SetActive(false);
        coffeeDoor.SetActive(false);
        fakeCoffeeDoor.SetActive(false);
        type =LevelType.Coffee;

        LightAdjust(true);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            if (VideoManager.Instance!=null&&VideoManager.Instance.videoPlayer.isPlaying)
            {
                VideoManager.Instance.StopVideo();
                return;
            }
            index++;
            if (index == 1)
            {
                GotoLevelTwo();
                
            }
            else if (index == 2)
            {
                GotoLevelThree();
            }
            else if (index == 3)
            {
                LevelPass();
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")&&type==LevelType.Coffee)
        {
            GotoLevelTwo();
        }
    }

    private void GotoLevelTwo()
    {
        player.canFeather = true;
        AnimationCompleteEventArgs args = new AnimationCompleteEventArgs(
            "按E拔羽毛",
            "目标:  修复灯塔，开灯寻找线索 \n 左键:  啄 \n 右键:  鸣叫 \n E键:  拔一根羽毛"
        );

        trueCoffeeLab.transform.position += new Vector3(0, 50, 0);
        //trueCoffeeLab.SetActive(false);
        fakeCoffeeLab.SetActive(true);
        persons.SetActive(true);
        parrot.SetActive(true);

        LightAdjust(false);

        player.transform.position = level2Point.position;
        MainCamara.Instance.SetPosition(player.transform.position);
        type = LevelType.Beach;

        coffeeDoor.SetActive(true);
        fakeCoffeeDoor.SetActive(true);

        if(VideoManager.Instance != null && VideoManager.Instance.gameObject.activeSelf)
        {
            VideoManager.Instance.PlayVideoClip(VideoManager.Instance.electricity, args);
        }
        else
        {
            TutorialManager.instance.ConfigureAndStartSimplifiedPrompt(args);
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
        persons.SetActive(false);
        shadowObject.SetActive(true);
        fakeCoffeeLab.transform.localPosition = new Vector3(-45.51f, 15.2f, 23.16f);


        type = LevelType.Three;

        if (animatorController != null)
        {
            player.GetComponent<Animator>().runtimeAnimatorController = animatorController;
        }

        if (VideoManager.Instance != null && VideoManager.Instance.gameObject.activeSelf)
        {
            VideoManager.Instance.PlayVideoClip(VideoManager.Instance.findFeet, args);
        }
        else
        {
            TutorialManager.instance.ConfigureAndStartSimplifiedPrompt(args);
        }
    }

     public void LevelPass()
     {
        VideoManager.Instance.PlayVideoClip(VideoManager.Instance.end,null);
     }

    public void LightAdjust(bool isCoffee)
    {
        if (isCoffee)
        {
            coffeeLight.SetActive(true);
            beachLight.SetActive(false);
        }
        else
        {
            coffeeLight.SetActive(false);
            beachLight.SetActive(true);
        }
    }

}

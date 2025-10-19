using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgrammerInteractive : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }

    public bool Disposable { get; set; } = true;
    Player player;
    ProgrammerController controller;
    FlipVisualController flipVisualController;
    public Vector3 dialogOffset;
    public GameObject computer;
    private void Start()
    {
        player=Player.Instance;
        controller = GetComponent<ProgrammerController>();
        flipVisualController = GetComponent<FlipVisualController>();
    }
    public bool Interact()
    {
        //IInteractive interactive = player.GetCurrentInteractiveItem();
        //if (interactive != null)
        //{
        //    if (interactive.instance.name == "CoffeeCup(Clone)")
        //    {
        //        player.OverInteractive();
        //        //Debug.Log("哦！谢谢你的咖啡");
        //        DialogManager.Instance.ShowDialog("哪来的咖啡", transform.position, dialogOffset, this.transform);
        //        Destroy(interactive.instance.gameObject);
        //        StartCoroutine(DrinkCoffeeAndThenGoToToilet());
        //        return true;
        //    }
        //    else if (interactive.instance.name == "CoffeeBean")
        //    {
        //        DialogManager.Instance.ShowDialog("我不是蘑菇=(", transform.position, dialogOffset, this.transform);
        //    }
        //    else if(interactive.instance.name == "CelesteStrawberry")
        //    {
        //        DialogManager.Instance.ShowDialog("这只有一个草莓  不够加一条命", transform.position, dialogOffset, this.transform);
        //    }
        //    return false;
        //}
        //else
        //{
            float value=Random.Range(0f, 1f);
            if (value < 0.5f)
            {
                DialogManager.Instance.ShowDialog("又要加班到四点", transform.position, dialogOffset, this.transform);
            }
            else if(value < 0.8f)
            {
                DialogManager.Instance.ShowDialog("程序员的生活就是不断地debug", transform.position, dialogOffset, this.transform);
            }
            else if(value<0.95f)
            {
                DialogManager.Instance.ShowDialog("来世要进幻想乡", transform.position, dialogOffset, this.transform);
            }
            else
            {
                DialogManager.Instance.ShowDialog("超级黑客 不是超级嗨客", transform.position, dialogOffset, this.transform);
            }
            return false;
        //}
    }

    public IEnumerator DrinkCoffeeAndThenGoToToilet()
    {
        StartCoroutine(flipVisualController.FlipThenChangeSprite("Animation/NPC_Programmer/State/DrinkCoffee"));
        yield return new WaitForSeconds(10f);
        DialogManager.Instance.ShowDialog("呸！真倒霉！这咖啡有毒！！", transform.position, dialogOffset, this.transform);
        controller.SetActiveRoute(ProgrammerController.RouteType.路线1);
    }

    public IEnumerator ComputerBoom()
    {
        yield return new WaitForSeconds(5f);
        //StartCoroutine(flipVisualController.FlipThenChangeSprite("Animation/NPC_Programmer/State/ComputerBroken"));
        Destroy(computer);
        DialogManager.Instance.ShowDialog("靠！我刚写的程序！！！！！！！！", transform.position, dialogOffset, this.transform);
        controller.SetActiveRoute(ProgrammerController.RouteType.路线2);
    }
    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }
}

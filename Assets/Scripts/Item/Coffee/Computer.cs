using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Computer : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }

    Player player=Player.Instance;
    public ProgrammerInteractive programmerInteractive;
    public bool Disposable { get; set; } = true;

    private void Start()
    {
        player= Player.Instance;
        //programmerInteractive=transform.parent.GetComponent<ProgrammerInteractive>();   
    }
    public bool Interact()
    {
        if (player.GetCurrentInteractiveItem() != null)
        {
            IInteractive interacitveItem = player.GetCurrentInteractiveItem();
            string interactiveItemName = interacitveItem.instance.name;
            if (interactiveItemName == "CoffeeCup(Clone)")
            {
                player.OverInteractive();
                DialogManager.Instance.ShowDialog("你对我的电脑做了什么", transform.position, new Vector3(2, 2.5f, 0), this.transform);
                Destroy(interacitveItem.instance.gameObject);
                programmerInteractive.StartCoroutine(programmerInteractive.ComputerBoom());
                return true;
            }
            else
            {
                //Debug.Log("这台电脑需要一杯咖啡来启动！");
                DialogManager.Instance.ShowDialog("这台电脑需要一杯咖啡来启动！", transform.position, new Vector3(2, 2.5f, 0), this.transform);
                return false;
            }
        }
        else
        {
            //Debug.Log("这是一台电脑，我想你知道它是用来干嘛的");
            DialogManager.Instance.ShowDialog("程序员的电脑  还是不要乱动了吧", transform.position, new Vector3(2, 2.5f, 0), this.transform);
            return false;
        }
    }

    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }
}

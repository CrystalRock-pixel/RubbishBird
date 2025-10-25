using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgrammerReactToCall : ReactToCall
{
    public CoffeeTable coffeeTable;
    public ProgrammerInteractive programmerInteractive;
    private Vector3 dialogOffset = new Vector3(0, 2.5f, 0);

    public void Start()
    {
        programmerInteractive = transform.GetComponent<ProgrammerInteractive>();
        dialogOffset = programmerInteractive.dialogOffset;
    }
    public override void React()
    {
        base.React();
        IInteractive interactive = coffeeTable.currentItem;
        if (interactive != null)
        {
            string itemName = interactive.instance.name;
            if (itemName=="CoffeeCup(Clone)"||itemName=="MilkCup(Clone)")
            {
                DialogManager.Instance.ShowDialog("啊啊啊 哪来的鸟  啊啊啊啊啊 我的电脑", transform.position, dialogOffset, this.transform);
                Destroy(interactive.instance.gameObject);
                StartCoroutine(programmerInteractive.ComputerBoom());
            }
            else if (itemName=="BadCoffeeCup(Clone)")
            {
                DialogManager.Instance.ShowDialog("预制咖啡吗   来得这么快", transform.position, dialogOffset, this.transform);
                Destroy(interactive.instance.gameObject);
                StartCoroutine(programmerInteractive.DrinkCoffeeAndThenGoToToilet());
            }
            else if (itemName == "CoffeeBean")
            {
                DialogManager.Instance.ShowDialog("哪来的咖啡豆  如果我是蘑菇就好了", transform.position, dialogOffset, this.transform);
            }
            else if (itemName == "CelesteStrawberry")
            {
                DialogManager.Instance.ShowDialog("这只有一个草莓  不够加一条命", transform.position, dialogOffset, this.transform);
            }
            else if(itemName.Contains("DiamondPickaxe"))
            {
                DialogManager.Instance.ShowDialog("真想挖黑曜石  然后逃进下界", transform.position, dialogOffset, this.transform);
            }
            else 
            {
                DialogManager.Instance.ShowDialog("谁在我桌子上丢垃圾呜呜呜QAQ", transform.position, dialogOffset, this.transform);
            }
        }
        else
        {
            float value = Random.Range(0f, 1f);
            if (value < 0.5f)
            {
                DialogManager.Instance.ShowDialog("出幻觉了吗  哪来的鸟叫", transform.position, dialogOffset, this.transform);
            }
            else if (value < 0.8f)
            {
                DialogManager.Instance.ShowDialog("工作 工作 工作", transform.position, dialogOffset, this.transform);
            }
            else
            {
                DialogManager.Instance.ShowDialog("森罗万象出新专辑了  嘿嘿嘿", transform.position, dialogOffset, this.transform);
            }
        }
    }
}

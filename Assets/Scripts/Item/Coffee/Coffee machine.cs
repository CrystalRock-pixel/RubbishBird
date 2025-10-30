using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coffeemachine : MonoBehaviour, IInteractive
{
    public bool inLevell3 = false;
    private bool inSynthesis = false;
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }
    public bool Disposable { get; set; } = true;

    Player player;

    private void Start()
    {
        player=Player.Instance;
    }
    public bool Interact()
    {
        if (player.GetCurrentInteractiveItem() != null)
        {
            IInteractive interacitveItem = player.GetCurrentInteractiveItem();
            string interactiveItemName = interacitveItem.instance.name;
            if (interactiveItemName .Contains("CoffeeBean")&&LevelManager.instance.type!=LevelManager.LevelType.Three)
            {
                player.OverInteractive();
                //Debug.Log("制作咖啡中...");
                DialogManager.Instance.ShowDialog("制作咖啡中...", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                Destroy(interacitveItem.instance.gameObject);
                StartCoroutine(MakeThing(Resources.Load<GameObject>("Prefabs/Item/CoffeeCup")));
                return true;
            }
            else if (interactiveItemName .Contains("BadCoffeeBean") && LevelManager.instance.type != LevelManager.LevelType.Three)
            {
                player.OverInteractive();
                //Debug.Log("制作咖啡中...");
                DialogManager.Instance.ShowDialog("制..制.作...咖咖..咖啡中...", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                Destroy(interacitveItem.instance.gameObject);
                StartCoroutine(MakeThing(Resources.Load<GameObject>("Prefabs/Item/BadCoffeeCup")));
                return true;
            }
            else if (interactiveItemName .Contains("CelesteStrawberry"))
            {
                //Debug.Log("正宗塞莱斯特草莓应该集齐202颗，并用于制作草莓派");
                DialogManager.Instance.ShowDialog("正宗塞莱斯特草莓应该集齐202颗  并用于制作草莓派", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                return false;
            }
            else if (interactiveItemName.Contains("BaristaToy"))
            {
                DialogManager.Instance.ShowDialog("材质解析中--------99% \n 成分解析中-------99% \n", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                Destroy(interacitveItem.instance.gameObject);
                StartCoroutine(MakeThing(Resources.Load<GameObject>("Prefabs/Item/DiamondPickaxe")));
                return true;
            }
            else if (inSynthesis)
            {
                if (interactiveItemName.Contains("FirstWater") || interactiveItemName.Contains("FirstCoffeeBean"))
                {
                    DialogManager.Instance.ShowDialog("正在合成中...", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                    Destroy(interacitveItem.instance.gameObject);
                    StartCoroutine(MakeThing(Resources.Load<GameObject>("Prefabs/Item/DataCoffee")));
                    inSynthesis = false;
                    return true;
                }
                DialogManager.Instance.ShowDialog("需要起源之豆和源初之水", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                return false;
            }
            else if (interactiveItemName.Contains("CoffeeBean") && LevelManager.instance.type == LevelManager.LevelType.Three)
            {
                DialogManager.Instance.ShowDialog("已收集起源之豆", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                inSynthesis = true;
                Destroy(interacitveItem.instance.gameObject);
                return true;
            }
            else if (interactiveItemName.Contains("FirstWater"))
            {
                DialogManager.Instance.ShowDialog("已收集源初之水", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                inSynthesis = true;
                Destroy(interacitveItem.instance.gameObject);
                return true;
            }
            else
            {
                //Debug.Log("需要咖啡豆才能制作咖啡！");
                DialogManager.Instance.ShowDialog("需要咖啡豆才能制作咖啡！", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                return false;
            }
        }
        else
        {
            //Debug.Log("这是一个咖啡机，我想你知道它是用来干嘛的");
            DialogManager.Instance.ShowDialog("这是一个咖啡机  你知道它是用来干嘛的", transform.position, new Vector3(0, 2.5f, 0), this.transform);
            return false;
        }
    }
    IEnumerator MakeThing(GameObject prefab)
    {
        yield return new WaitForSeconds(5f);
        GameObject gameObject = Instantiate(prefab, transform.position, prefab.transform.rotation);
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 2f, -4f);
    }

    public void InteractEnd()
    {
        
    }
}

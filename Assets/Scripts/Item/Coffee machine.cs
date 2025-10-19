using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coffeemachine : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }

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
            if (interactiveItemName == "CoffeeBean")
            {
                player.OverInteractive();
                //Debug.Log("制作咖啡中...");
                DialogManager.Instance.ShowDialog("制作咖啡中...", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                Destroy(interacitveItem.instance.gameObject);
                StartCoroutine(MakeCoffee());
                return true;
            }
            else if (interactiveItemName == "CelesteStrawberry")
            {
                //Debug.Log("正宗塞莱斯特草莓应该集齐202颗，并用于制作草莓派");
                DialogManager.Instance.ShowDialog("正宗塞莱斯特草莓应该集齐202颗  并用于制作草莓派", transform.position,new Vector3(0,2.5f,0),this.transform);
                return false;
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

    IEnumerator MakeCoffee()
    {
        yield return new WaitForSeconds(5f);
        GameObject gameObject = Instantiate(Resources.Load<GameObject>("Prefabs/Item/CoffeeCup"), transform.position, Quaternion.identity);
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 2f, -4f);
    }

    public void InteractEnd()
    {
        
    }
}

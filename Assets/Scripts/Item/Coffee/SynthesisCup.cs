using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynthesisCup : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }
    public bool Disposable { get => true; set => throw new System.NotImplementedException(); }

    private Player player=>Player.Instance;
    int CustomeInteractiveCount = 0;
    public bool Interact()
    {
        if (player.GetCurrentInteractiveItem() != null)
        {
            IInteractive interacitveItem = player.GetCurrentInteractiveItem();
            string name = interacitveItem.instance.name;
            if (name == "Milk")
            {
                player.OverInteractive();
                //Debug.Log("制作咖啡中...");
                DialogManager.Instance.ShowDialog("倒牛奶 \n 倒洒了也没有必要为他哭泣", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                Destroy(interacitveItem.instance.gameObject);
                StartCoroutine(ChangeSelfToPrefab(Resources.Load<GameObject>("Prefabs/Item/MilkCup")));
                return true;
            }
            else if(name== "CoffeeBean"||name=="BadCoffeeBean")
            {
                DialogManager.Instance.ShowDialog("咖啡豆不好喝", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                return false;
            }
            else if(name== "BaristaToy")
            {
                DialogManager.Instance.ShowDialog("咖啡师放进杯子里也做不出来咖啡食", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                return false;
            }
            else
            {
                DialogManager.Instance.ShowDialog("这东西不能倒进杯子里", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                return false;
            }
        }
        else
        {
            CustomeDialog();
            return false;
        }
    }

    void CustomeDialog()
    {
        if(CustomeInteractiveCount == 0)
        {
            DialogManager.Instance.ShowDialog("这是一个空杯子  里面没有薯条", transform.position, new Vector3(0, 2.5f, 0), this.transform);
            CustomeInteractiveCount++;
        }
        else if (CustomeInteractiveCount == 1)
        {
            DialogManager.Instance.ShowDialog("也许可以装点什么喝的？", transform.position, new Vector3(0, 2.5f, 0), this.transform);
            CustomeInteractiveCount++;
        }
        else
        {
            DialogManager.Instance.ShowDialog("空杯子还是空杯子  \n 你盯着它 \n 它也依然只是一只小小的杯子", transform.position, new Vector3(0, 2.5f, 0), this.transform);
        }
    }

    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }
    IEnumerator ChangeSelfToPrefab(GameObject thingPrefab)
    {
        yield return new WaitForSeconds(1f);
        Instantiate(thingPrefab, transform.position, thingPrefab.transform.rotation);
        player.interactiveItems.Remove(this);
        GetComponent<BoxCollider>().enabled = false;
        gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintingHand : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }
    public bool Disposable { get => true; set => throw new System.NotImplementedException(); }

    Player player;
    private GameObject hand;
    private void Start()
    {
        player = Player.Instance;
        hand=transform.GetChild(0).gameObject;
    }
    public bool Interact()
    {
        IInteractive interactive= player.GetCurrentInteractiveItem();
        if (interactive != null)
        {
            string name = interactive.instance.name;
            if (name.Contains("DataCoffee"))
            {
                DialogManager.Instance.ShowDialog("数据流拿铁神奇的修复了bug", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                hand.SetActive(false);
                player.haveHand = true;
                return true;
            }
            else
            {
                DialogManager.Instance.ShowDialog("这个可修不了bug", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                return false;
            }
        }
        else
        {
            DialogManager.Instance.ShowDialog("诡异的名画  诡异的手", transform.position, new Vector3(0, 2.5f, 0), this.transform);
            return false;
        }
    }

    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sea : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }
    public bool Disposable { get; set; } = false;
    public bool Interact()
    {
        IInteractive interactive = Player.Instance.GetCurrentInteractiveItem();
        if (interactive != null && interactive.instance.name .Contains("Cup"))
        {
            //DialogManager.Instance.ShowDialog("从水下第一个生命的萌芽开始...", transform.position, new Vector3(0, 2, 0), this.transform);
            Transform inteTransform = interactive.instance;
            inteTransform.name = "FirstWater";
            //inteTransform.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Item/Water/FirstWater");
            return false;
        }
        else
        {
            //DialogManager.Instance.ShowDialog("大海啊，大海，你是多么的宽广", transform.position, new Vector3(0, 2, 0), this.transform);
            return false;
        }
    }

    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }
}

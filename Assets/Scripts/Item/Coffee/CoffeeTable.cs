using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeTable : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }

    public Transform tableItemPos;
    public IInteractive currentItem;
    private Player player;
    public bool Disposable { get; set; } = true;
    private void Start()
    {
        player = Player.Instance;
    }
    public bool Interact()
    {
        if (player.GetCurrentInteractiveItem() != null)
        {
            if (currentItem == null)
            {
                IInteractive interacitveItem = player.GetCurrentInteractiveItem();
                interacitveItem.InteractEnd();
                player.OverInteractive();
                interacitveItem.instance.position = tableItemPos.position;
                interacitveItem.instance.rotation = tableItemPos.rotation;
                currentItem = interacitveItem;
                return false;
            }
            else
            {
                //Debug.Log("桌子上已经有东西了");
                DialogManager.Instance.ShowDialog("桌子上已经有东西了", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                return false;
            }
        }
        else
        {
            if (currentItem != null)
            {
                player.TryInteracitveWithItem(currentItem);
                currentItem = null;
                return true;
            }
            else
            {
                //Debug.Log("桌子上没有东西可以拿");
                DialogManager.Instance.ShowDialog("桌子上没有东西可以拿", transform.position, new Vector3(0, 2.5f, 0), this.transform);
                return false;
            }
        }
    }

    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }
}

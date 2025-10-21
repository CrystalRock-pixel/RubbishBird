using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeesawInteractive : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }
    public bool Disposable { get => true; set => throw new System.NotImplementedException(); }
    public GameObject currentObject;
    public bool isLeftSide = true;
    public SeesawController seesawController;
    public Player player;

    private void Start()
    {
        seesawController =transform.parent.GetComponentInParent<SeesawController>();
        player =Player.Instance;
    }

    public bool Interact()
    {
        IInteractive heldItem=player.GetCurrentInteractiveItem();
        if (currentObject == null&&heldItem!=null)
        {
            heldItem.InteractEnd();
            player.OverInteractive();
            currentObject = heldItem.instance.gameObject;
            seesawController.PlaceItem(currentObject, isLeftSide);
            return false;
        }
        else if(currentObject!=null&&heldItem==null)
        {
            seesawController.RemoveItem(isLeftSide);
            player.TryInteracitveWithItem(currentObject.GetComponent<IInteractive>());
            currentObject = null;
            return true;
        }
        else if (currentObject == null && heldItem == null)
        {
            DialogManager.Instance.ShowDialog("或许你可以用它来当投石机", transform.position, new Vector3(0, 2.5f, 0), this.transform);
            return false;
        }
        else
        {
            DialogManager.Instance.ShowDialog("你有一个绝妙的过关方法  可是翘翘板的空间太小  你放不下", transform.position, new Vector3(0, 2.5f, 0), this.transform);
            return false;
        }
    }

    public void SetCurrentObjectNull()
    {
        currentObject = null;
    }
    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }
}

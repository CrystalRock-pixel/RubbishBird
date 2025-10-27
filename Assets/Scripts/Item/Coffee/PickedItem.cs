using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickedItem : MonoBehaviour, IInteractive
{
    protected Player player = Player.Instance;
    public bool Disposable { get; set; } = false;
    Quaternion oriRatation;
    Transform IInteractive.instance
    {
        get { return this.transform; }
        set { }
    }

    protected virtual void Start()
    {
        player = Player.Instance;
        oriRatation = transform.rotation;
    }
    public virtual bool Interact()
    {
        IInteractive interactive = player.GetCurrentInteractiveItem();
        if (interactive == null)
        {
            LosePhysic();
            transform.position = player.transform.GetChild(0).position;
            transform.rotation = oriRatation;
            transform.SetParent(player.transform.GetChild(0));
            return true;
        }
        else
        {
            DialogManager.Instance.ShowDialog("嘴里叼不下东西啦", transform.position, new Vector3(0, 2.5f, 0), this.transform);
            return false;
        }
    }

    protected virtual void LosePhysic()
    {
        if(transform.GetChild(0).GetComponent<Collider>() != null)   //第一个子物体是碰撞体
        {
            transform.GetChild(0).GetComponent<Collider>().enabled = false;
        }
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    public virtual void InteractEnd()
    {
        Placed();
        player.OverInteractive();
    }
    public virtual void Placed()
    {
        if (transform.GetChild(0).GetComponent<Collider>() != null)
        {
            transform.GetChild(0).GetComponent<Collider>().enabled = true;
        }
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = false;
        }
        transform.SetParent(null);
        transform.position = player.transform.position;
    }
}

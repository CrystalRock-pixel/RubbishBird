using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickedItem : MonoBehaviour, IInteractive
{
    Player player = Player.Instance;

    Transform IInteractive.instance
    {
        get { return this.transform; }
        set { }
    }

    private void Start()
    {
        player = Player.Instance;
    }
    public void Interact()
    {
        LosePhysic();
        transform.position = player.transform.GetChild(0).position;
        transform.SetParent(player.transform.GetChild(0));
    }

    void LosePhysic()
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

    void IInteractive.InteractEnd()
    {
        Placed();
        player.OverInteractive();
    }
    public void Placed()
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

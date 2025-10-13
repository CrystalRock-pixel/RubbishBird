using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragItem : MonoBehaviour, IInteractive
{
    public float mass = 1f;

    [HideInInspector]
    public float speedScale = 1f;

    Player player;
    Rigidbody rb;
    Transform IInteractive.instance
    {
        get { return this.transform; }
        set { }
    }

    private void Start()
    {
        mass = 1f;
        player = Player.Instance;
        speedScale = 1 / (mass * 2);
        rb = GetComponent<Rigidbody>();
    }
    public void Interact()
    {
        transform.SetParent(player.transform);
        rb.isKinematic = true;
        StartCoroutine(SetPlayerPickedItem());
        Debug.Log("ÍÏ×§£¡");
    }

    public void Placed()
    {
        transform.SetParent(null);
        rb.isKinematic=false;
        Debug.Log("ËÉ×ì£¡");
    }

    IEnumerator SetPlayerPickedItem()
    {
        yield return new WaitForEndOfFrame();
        player.PickUpItem(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragItem : MonoBehaviour, IInteractive
{
    public float mass = 1f;

    [HideInInspector]
    public float speedScale = 1f;

    private bool isInteracting = false;

    Player player;
    Rigidbody rb;
    public bool Disposable { get; set; } = false;
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
        isInteracting = false;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }
    public bool Interact()
    {
        IInteractive interactive = player.GetCurrentInteractiveItem();
        if (interactive != null)
        {
            return false;
        }
        else
        {
            transform.SetParent(player.transform);
            rb.isKinematic = true;
            //rb.isKinematic = false;
            isInteracting = true;
            Debug.Log("ÍÏ×§£¡");
            return true;
        }
    }

    void IInteractive.InteractEnd()
    {
        if (isInteracting)
        {
            Placed();
            player.OverInteractive();
            isInteracting = false;
        }
    }
    public void Placed()
    {
        transform.SetParent(null);
        //rb.isKinematic = false;
        //rb.isKinematic = true;
        Debug.Log("ËÉ×ì£¡");
    }
}

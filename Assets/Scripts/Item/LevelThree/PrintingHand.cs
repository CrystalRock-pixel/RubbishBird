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
            DialogManager.Instance.ShowDialog("嘴里叼着东西可修不了bug", transform.position, new Vector3(0, 2.5f, 0), this.transform);
            return false;
        }
        else
        {
            hand.SetActive(false);
            return true;
        }
    }

    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }
}

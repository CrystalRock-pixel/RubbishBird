using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransmitObject : MonoBehaviour,IInteractive
{
    private BoxCollider interacteCollider;
    public Transform transmitSpot;
    public void Interact()
    {
        Player player = Player.Instance;
        player.transform.position = transmitSpot.position;
    }

    private void Start()
    {
        
    }
}

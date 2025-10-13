using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBox : MonoBehaviour,IInteractive
{

    private BoxCollider interacteCollider;
    public Transform transmitSpot;
    public void Interact()
    {
        Player player = Player.Instance;
        transform.position = player.transform.position+new Vector3(0,3,0);
    }

    private void Start()
    {

    }
}

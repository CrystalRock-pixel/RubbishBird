using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ˮ��ͷ
/// </summary>
public class Faucet :MonoBehaviour,IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }

    public GameObject Pond;    //ˮ̶

    private void Start()
    {
        Pond.SetActive(false);
    }
    public bool Interact()
    {
        Pond.SetActive(true);
        return false;
    }

    public void InteractEnd()
    {
        
    }
}

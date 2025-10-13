using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IInteractive
{
    // 当玩家按下 E 键时执行的实际逻辑
    void Interact();
}
public class InteractiveHandler:MonoBehaviour
{
    public BoxCollider trigger { get; set; }
    public string intercatObjectTag;
    public bool isInteracting=false;
    private void Start()
    {
        trigger = GetComponent<BoxCollider>();
    }
    private void Update()
    {
    }
    private void OnTriggerStay(Collider other)
    {
        //Debug.Log(other.name);
        if (other.CompareTag(intercatObjectTag))
        {
            if (this.GetComponent<IInteractive>() != null&& Input.GetKeyDown(KeyCode.E))
            {
                this.GetComponent<IInteractive>().Interact();
            }
        }
    }
}

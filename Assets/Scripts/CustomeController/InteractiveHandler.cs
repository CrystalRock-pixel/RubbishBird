using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IInteractive
{
    // 当玩家按下 E 键时执行的实际逻辑
    Transform instance { get; set; }
    bool Interact();//返回是否成功交互
    void InteractEnd();

    bool Disposable { get;set; }  //是否一次性交互，一次性交互即一次交互便交互完成
}
public class InteractiveHandler:MonoBehaviour
{
    public BoxCollider trigger { get; set; }
    public string intercatObjectTag;
    public bool canInteracting=false;

    public GameObject outlineObject;
    private void Start()
    {
        trigger = GetComponent<BoxCollider>();
        if (outlineObject != null)
        {
            outlineObject.SetActive(false);
        }
    }
    private void Update()
    {
    }
    private void OnTriggerStay(Collider other)
    {
        //Debug.Log(other.name);
        if (other.CompareTag(intercatObjectTag))
        {
            if (outlineObject != null)
            {
                outlineObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(intercatObjectTag))
        {
            if (outlineObject != null)
            {
                outlineObject.SetActive(false);
            }
        }
    }
}

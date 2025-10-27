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
    int Weight
    {
        get => 1;
    }
}
public class InteractiveHandler:MonoBehaviour
{
    public BoxCollider trigger { get; set; }
    public string interactObjectTag="Player";
    public bool canInteracting=false;

    public GameObject outlineObject;
    public Sprite sprite;
    private void Start()
    {
        trigger = GetComponent<BoxCollider>();
        if (outlineObject != null)
        {
            outlineObject.SetActive(false);
        }
        interactObjectTag = "Player";
    }
    private void Update()
    {
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(interactObjectTag))
        {
            if (outlineObject != null)
            {
                outlineObject.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(interactObjectTag))
        {
            if (sprite != null)
            {
                DialogManager.Instance.ShowBubble(sprite, other.transform,DialogManager.Instance.bubblePlayerOffset);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(interactObjectTag))
        {
            if (outlineObject != null)
            {
                outlineObject.SetActive(false);
            }
        }
    }
}

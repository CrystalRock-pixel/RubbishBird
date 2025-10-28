using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IInteractive
{
    // ����Ұ��� E ��ʱִ�е�ʵ���߼�
    Transform instance { get; set; }
    bool Interact();//�����Ƿ�ɹ�����
    void InteractEnd();

    bool Disposable { get;set; }  //�Ƿ�һ���Խ�����һ���Խ�����һ�ν����㽻�����
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomeDialogueNPC : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }
    public bool Disposable { get => true; set => throw new System.NotImplementedException(); }
    public Vector3 dialogOffset;
    public List<string> dialogContents;
    public bool Interact()
    {
        int randomIndex = Random.Range(0, dialogContents.Count);
        DialogManager.Instance.ShowDialog(dialogContents[randomIndex], transform.position, dialogOffset, this.transform);
        return false;
    }

    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }
}

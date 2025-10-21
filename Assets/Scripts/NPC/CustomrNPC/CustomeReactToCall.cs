using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomeReactToCall : ReactToCall
{
    public Vector3 dialogOffset;
    public List<string> dialogContents;
    public override void React()
    {
        base.React();
        int randomIndex = Random.Range(0, dialogContents.Count);
        DialogManager.Instance.ShowDialog(dialogContents[randomIndex], transform.position, dialogOffset, this.transform);
    }
}

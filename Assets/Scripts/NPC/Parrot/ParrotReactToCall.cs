using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParrotReactToCall : ReactToCall
{
    Parrot parrot;
    private void Start()
    {
        parrot = GetComponent<Parrot>();
    }
    public override void React()
    {
        base.React();
        parrot.triggerRoute3 = true;
        DialogManager.Instance.ShowDialog("啊救命  我再也不干坏事了", transform.position, new Vector3(0, -2.5f, 0), this.transform);
    }
}

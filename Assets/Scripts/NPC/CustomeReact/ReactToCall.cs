using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactToCall : MonoBehaviour
{
    public virtual void React()
    {
        Debug.Log(transform.name + " React to Call");
    }
}

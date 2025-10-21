using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ceshi : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // ¼ì²âÊó±ê×ó¼üµã»÷
        if (Input.GetMouseButtonDown(0))
        {
            AtmosphereManager.Instance.GlobalAngerTrigger = true;
        }
    }
}

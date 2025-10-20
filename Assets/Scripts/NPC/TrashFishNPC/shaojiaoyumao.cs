using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shaojiaoyumao : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("ÉÕ½¹ÓðÃ«Éú³É");
        AtmosphereManager.Instance.SetAttractionItem(this.transform);

    }


}

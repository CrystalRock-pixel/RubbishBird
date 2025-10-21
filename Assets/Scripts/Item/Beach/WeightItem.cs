using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightItem : PickedItem,IInteractive
{
    int IInteractive.Weight
    {
        get { return weight; } // ·µ»Ø×Ö¶ÎµÄÖµ (5)
    }

    [SerializeField]
    private int weight = 1;
    protected override void Start()
    {
        base.Start();
    }
}


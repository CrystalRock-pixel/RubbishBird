using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeNPCReactToCall : ReactToCall
{
    CoffeeNPC coffeeNPC;

    private void Start()
    {
        coffeeNPC = GetComponent<CoffeeNPC>();
    }
    public override void React()
    {
        base.React();
        coffeeNPC.ReachToBirdCall();
    }
}

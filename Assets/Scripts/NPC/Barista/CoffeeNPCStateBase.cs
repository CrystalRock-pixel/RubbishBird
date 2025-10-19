using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC 状态基类，继承自用户提供的 StateBase，并持有宿主 CoffeeNPC 引用。
/// </summary>
public class CoffeeNPCStateBase : StateBase
{
    // 宿主对象引用
    protected CoffeeNPC Npc { get; private set; }

    public CoffeeNPCStateBase(StateMachine stateMachine, CoffeeNPC npc) : base(stateMachine)
    {
        Npc = npc;
    }

    // 可以在这里实现通用的 NPC 状态逻辑
}

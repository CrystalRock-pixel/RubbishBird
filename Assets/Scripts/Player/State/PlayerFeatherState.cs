using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFeatherState : PlayerStateBase
{
    public PlayerFeatherState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected override Animator animator => base.animator;
    
    float existTime = 1f;
    public override void OnEnter()
    {
        base.OnEnter();
        animator.SetTrigger("Feather");
        player.InstantiateFeather(1.1f);
    }

    public override void OnExit()
    {
        base.OnExit();
        existTime = 1f;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        existTime -= Time.deltaTime;
        if (existTime <= 0f)
        {
            StateMachine.ChangeState(player.idleState);
        }
    }
}

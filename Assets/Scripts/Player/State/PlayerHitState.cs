using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitState : PlayerStateBase
{
    private Vector3 hitDirection;
    private float hitForce = 5f;
    private float rigidityTime = 1f;   //受击硬直时间
    public PlayerHitState(StateMachine stateMachine, Vector3 hitDirection, float hitForce) : base(stateMachine)
    {
        this.hitDirection = hitDirection;
        this.hitForce = hitForce;
    }
    protected override Animator animator => base.animator;

    public override void OnEnter()
    {
        base.OnEnter();
        Vector3 _kickbackVelocity;
        // 沿相反方向施加一个瞬时速度
        _kickbackVelocity = hitDirection.normalized * hitForce;
        rb.velocity += _kickbackVelocity;

        IInteractive interactive=player.GetCurrentInteractiveItem();
        if (interactive != null)
        {
            interactive.InteractEnd();
            player.OverInteractive();
        }
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if(rb.velocity.magnitude < 0.1f||rigidityTime<=0.1f)
        {
            StateMachine.ChangeState(player.idleState);
        }
        else
        {
            rigidityTime -= Time.deltaTime;
        }
    }
}

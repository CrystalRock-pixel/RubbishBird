using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStateBase : StateBase
{
    public PlayerStateBase(StateMachine stateMachine) : base(stateMachine)
    {
    }

    protected Player player => Player.Instance;
    protected override Animator animator => player.animator;
    protected Rigidbody rb => player.rb;
    protected bool isGrounded => player.isGrounded;

    protected bool canJump => player.canJump;

    protected bool canFeather => player.canFeather;

    protected AudioSource audioSource => player.audioSource;

    public override void OnEnter()
    {
        base.OnEnter();
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
    }
}

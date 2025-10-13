using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状态基类：所有具体状态都应继承它。
/// </summary>
public class StateBase
{
    // 状态机引用，用于通知状态机切换状态
    protected StateMachine StateMachine { get; private set; }
    protected virtual Animator animator { get;private set; }

    // 宿主对象（如 PlayerController）的引用，需要由子类在构造函数中自行转换类型
    // 在这里我们先不定义，让子类根据需要自己定义泛型或持有引用。
    // 为了简单，我们只传递 StateMachine。

    public StateBase(StateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    // ========== 状态生命周期方法 (都使用 virtual 方便统一管理和子类重写) ==========

    /// <summary>
    /// 进入状态时调用。
    /// </summary>
    public virtual void OnEnter()
    {
        // 统一调试逻辑：方便追踪状态切换
        Debug.Log($"[FSM] 进入状态: {this.GetType().Name}");
    }

    /// <summary>
    /// 每帧调用（对应 Update）。
    /// </summary>
    public virtual void OnUpdate() { }

    /// <summary>
    /// 每固定帧调用（对应 FixedUpdate）。
    /// </summary>
    public virtual void OnFixedUpdate() { }

    /// <summary>
    /// 退出状态时调用。
    /// </summary>
    public virtual void OnExit()
    {
        // 统一调试逻辑
        Debug.Log($"[FSM] 退出状态: {this.GetType().Name}");
    }
}
/// <summary>
/// 状态机类：管理当前状态的生命周期和切换。
/// </summary>
public class StateMachine
{
    private StateBase _currentState;

    public void Initialize(StateBase startingState)
    {
        _currentState = startingState;
        _currentState.OnEnter();
    }

    public void Update()
    {
        _currentState?.OnUpdate();
    }

    public void FixedUpdate()
    {
        _currentState?.OnFixedUpdate();
    }

    /// <summary>
    /// 核心方法：切换状态。
    /// </summary>
    /// <param name="newState">新的状态实例。</param>
    public void ChangeState(StateBase newState)
    {
        if (_currentState == newState)
        {
            return;
        }

        _currentState?.OnExit();
        _currentState = newState;
        _currentState.OnEnter();
    }
}
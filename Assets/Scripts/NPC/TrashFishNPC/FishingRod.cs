
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingRod : MonoBehaviour
{
    [Header("扰动检测设置")]
    [Tooltip("导电鱼在一次连续移动中，超过此距离（单位）将视为触发扰动。")]
    public float disturbanceThreshold = 5f;

    [Tooltip("每次成功触发扰动后，重置距离计算的冷却时间。")]
    public float resetCooldown = 3f;

    [Header("联动对象引用")]
    [Tooltip("场景中的 ConnectionManager 实例，用于开启特殊连通模式。")]
    public ConnectionManager connectionManager;

    [Tooltip("场景中的 ElectricEel 脚本，用于触发其 Angry 状态。")]
    public ElectricEel electricEel;

    [Header("速度调整设置")]
    [Tooltip("靠近鱼时，玩家的移动速度缩放比例。")]
    public float movingTowardsEelScale = 1.2f; // 增加移速

    [Tooltip("远离鱼时，玩家的移动速度缩放比例。")]
    public float movingAwayFromEelScale = 0.8f; // 减慢移速

    private Player player; // 玩家引用
    private Transform playerHoldingPoint; // 玩家手/嘴的引用 

    private Vector3 lastPosition;       // 记录上次检查时的位置
    private float totalMovedDistance;   // 累积的远离鱼的总移动距离
    private bool isOnCooldown = false;   // 是否处于重置冷却期

    private void Start()
    {


        player = Player.Instance; // 获取玩家单例

        if (electricEel == null)
        {

            electricEel = FindObjectOfType<ElectricEel>();
        }

        if (player == null || electricEel == null)
        {
            Debug.LogError("FishingRod 缺少必要的 Player 或 ElectricEel 引用，功能无法正常工作。");
            enabled = false;
            return;
        }

        // 获取玩家持有物品的位置
        if (player.transform.childCount > 0)
        {
            // 鱼竿成为 的子物体时，即为被拖动/持有状态
            playerHoldingPoint = player.transform.GetChild(0);
        }
        else
        {
            Debug.LogError("Player缺少GetChild(0)作为持有物品的Transform，鱼竿拖动检测将失败。");
            enabled = false;
            return;
        }

        // 初始化位置
        lastPosition = transform.position;
    }

    private void Update()
    {

        bool isBeingHeld = (transform.parent == playerHoldingPoint);

        // 如果鱼竿没有被持有，或者处于冷却中，则恢复正常速度并退出本次Update
        if (!isBeingHeld || isOnCooldown)
        {
            if (player != null)
            {
                player.SetMoveSpeedScale(1.0f); // 确保非拖动状态下速度是正常的
            }
            if (!isBeingHeld)
            {
                // 如果没有被持有，重置累计距离，以便下次拖动时重新开始计算
                totalMovedDistance = 0;
            }
            return;
        }



        // 计算当前帧的移动距离 (只考虑 XZ 平面)
        Vector3 currentPosition = transform.position;
        Vector3 currentHorizontal = new Vector3(currentPosition.x, 0, currentPosition.z);
        Vector3 lastHorizontal = new Vector3(lastPosition.x, 0, lastPosition.z);

        Vector3 frameMovement = currentHorizontal - lastHorizontal;
        float frameDistance = frameMovement.magnitude;

        // 鱼和鱼竿（本对象）的水平位置
        Vector3 eelHorizontal = new Vector3(electricEel.transform.position.x, 0, electricEel.transform.position.z);
        Vector3 directionToEel = (eelHorizontal - currentHorizontal).normalized;

        // 玩家的水平输入方向

        Vector3 playerInputDirection = new Vector3(player.horizontalInput, 0, player.verticalInput);



        if (playerInputDirection.sqrMagnitude > 0.01f) // 确保玩家有输入
        {
            playerInputDirection.Normalize();

            // 使用点积判断移动方向是靠近还是远离
            // 靠近：点积 > 0 ； 远离：点积 < 0
            float dotProduct = Vector3.Dot(playerInputDirection, directionToEel);

            if (dotProduct > 0.1f) // 判定为“靠近”
            {
                // 靠近鱼，加速
                player.SetMoveSpeedScale(movingTowardsEelScale);
            }
            else if (dotProduct < -0.1f) // 判定为“远离”
            {
                // 远离鱼，减速
                player.SetMoveSpeedScale(movingAwayFromEelScale);

                // 累加“远离”鱼的移动距离
                totalMovedDistance += frameDistance;
            }
            else
            {
                // 横向移动或几乎静止，保持正常速度
                player.SetMoveSpeedScale(1.0f);
            }
        }
        else
        {
            // 玩家无输入，保持正常速度
            player.SetMoveSpeedScale(1.0f);
        }

        // 如果帧移动距离过大（如传送），则重置距离累积，避免误触发
        if (frameDistance > 1f)
        {
            totalMovedDistance = 0;
            player.SetMoveSpeedScale(1.0f); // 重置速度
        }

        lastPosition = currentPosition;



        // 检查是否达到扰动阈值
        if (totalMovedDistance >= disturbanceThreshold)
        {
            TriggerDisturbance();
        }
    }


    private void TriggerDisturbance()
    {

        if (connectionManager != null)
        {
            connectionManager.forceConnectedMode = true;
            connectionManager.CheckConnectivity();
            Debug.Log("【扰动触发】: 远离鱼的累计距离超过阈值。ConnectionManager 强制连通模式已开启!");
        }

        // 激怒
        if (electricEel != null)
        {
            electricEel.isAngryTriggered = true; // 设置外部触发愤怒的变量
            Debug.Log("【扰动触发】: ElectricEel 外部愤怒状态已激活!");
        }

        // 3. 玩家速度恢复正常
        if (player != null)
        {
            player.SetMoveSpeedScale(1.0f);
        }

        // 4. 启动冷却计时器
        StartCoroutine(StartCooldown());
    }


    private System.Collections.IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        totalMovedDistance = 0; // 重置计数
        yield return new WaitForSeconds(resetCooldown);
        isOnCooldown = false;
        Debug.Log("扰动监测已重置，可再次触发。");
    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, disturbanceThreshold / 2f);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishermanBasket : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject prefabToSpawn;        // 要弹出的预制体（例如：鱼、螃蟹等）
    public int spawnCount = 3;              // 每次弹出预制体的数量 【数量由这里严格控制】
    public float minSpawnForce = 3f;        // 弹出的最小随机力度
    public float maxSpawnForce = 6f;        // 弹出的最大随机力度
    public float spawnRadius = 0.5f;        // 弹出的随机半径 (控制分散程度)
    public float spawnDelay = 0.5f;         // 延迟多久后弹出（给渔夫反应时间）

    [Header("渔夫引用")]
    [Tooltip("拖拽场景中 FishermanNPC 脚本所在的物体")]
    public FishermanNPC fishermanNPC;

    private bool hasSpawned = false; // 标记是否已经生成过（避免重复生成）

    private void Start()
    {
        // 尝试自动查找 FishermanNPC
        if (fishermanNPC == null)
        {
            fishermanNPC = FindObjectOfType<FishermanNPC>();
            if (fishermanNPC == null)
            {
                Debug.LogError("FishermanBasket: 场景中找不到 FishermanNPC 脚本的引用! 无法工作。");
                enabled = false;
            }
        }

        if (prefabToSpawn == null)
        {
            Debug.LogError("FishermanBasket: PrefabToSpawn 未设置! 请设置要弹出的预制体。");
            enabled = false;
        }
    }

    private void Update()
    {
       
        if (fishermanNPC != null &&
            fishermanNPC.attachedChild != null &&
            fishermanNPC.attachedChild.transform.parent == null && // 检查是否被解锁
            !hasSpawned)
        {
            // 确保只触发一次
            hasSpawned = true;

            // 启动生成协程
            StartCoroutine(SpawnSequence());
        }
    }

    private IEnumerator SpawnSequence()
    {
        
        yield return new WaitForSeconds(spawnDelay);

     
        for (int i = 0; i < spawnCount; i++)
        {
           
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);

           
            Vector3 basePosition = transform.position + Vector3.up * 0.2f;
            Vector3 spawnPosition = basePosition + randomOffset;

            GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

            
            if (spawnedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
              
                Vector3 randomHorizontalDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

                Vector3 launchDirection = (randomHorizontalDir + Vector3.up).normalized;

             
                float force = Random.Range(minSpawnForce, maxSpawnForce);

                rb.AddForce(launchDirection * force, ForceMode.VelocityChange);
            }
        }

        Debug.Log("鱼篓已弹出物品。");
    }


    public void ResetBasket()
    {
        hasSpawned = false;
        StopAllCoroutines(); 
    }
}
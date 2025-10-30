using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AudioSource))]
public class PowerDistributionBox : MonoBehaviour
{
    [Header("音频设置")]
    [Tooltip("电流声的音频片段。该音频将非循环播放一次。")]
    public AudioClip currentSoundClip;

    private AudioSource audioSource;

    
    [Header("状态输出 (供外部系统读取)")]
    [Tooltip("【音频控制】当电路断开且音频正在播放或准备播放时为 True。播放完毕后自动重置为 False。")]
    public bool isCurrentSoundPlaying = false;

    [Tooltip("【配电箱状态】当电路连通 (已修复) 时为 True。")]
    public bool isFixed = false;

    [Header("检查设置")]
    [Tooltip("连通性检查的频率")]
    public float checkInterval = 0.2f;
    private float nextCheckTime;

    public WrenchSwitch wrenchSwitch;

    public GameObject cover_Open;
    public GameObject cover_Close;


    private bool hasStartedAudioSequence = false;

    private ConnectionManager connectionManager;

    private void Start()
    {
        // 1. 获取或添加 AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 2. 配置 AudioSource：设置为不循环
        audioSource.loop = false;
        if (currentSoundClip == null)
        {
            Debug.LogWarning("PowerDistributionBox: Current Sound Clip 未设置，音频功能将无法使用。", this);
        }

        // 3. 尝试获取 ConnectionManager 实例
        connectionManager = ConnectionManager.Instance;
        if (connectionManager == null)
        {
            Debug.LogError("PowerDistributionBox: 场景中找不到 ConnectionManager 实例，无法检查连通性！", this);
        }

        // 首次检查
        CheckStatus();
        nextCheckTime = Time.time + checkInterval;
    }

    private void Update()
    {
        // 如果 ConnectionManager 实例被 ElectricEel 激活，则在 Update 中再次捕获
        if (connectionManager == null)
        {
            connectionManager = ConnectionManager.Instance;
            if (connectionManager == null) return;
        }

        // 定时检查逻辑
        if (checkInterval > 0 && Time.time >= nextCheckTime)
        {
            CheckStatus();
            nextCheckTime = Time.time + checkInterval;
        }
        else if (checkInterval <= 0)
        {
            // 如果用户设置为 0 或负数，则每帧检查
            CheckStatus();
        }
    }


    private void CheckStatus()
    {
        if (connectionManager == null) return;


        bool isConnected = connectionManager.CheckConnectivity();

        if (isFixed)
        {
            Open(false);
        }
        else if (!isFixed)
        {
            Open(true);
        }
        else if(isConnected && !isFixed)
        {
            DialogManager.Instance.ShowDialog("配电箱已修复，电路恢复连通。", transform.position, new Vector3(0, 2.5f, 0), this.transform);
        }
        isFixed = isConnected;

        if (isConnected)
        {
          
            StopAllCoroutines();
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            // 协程被停止，isCurrentSoundPlaying 自动重置为 False (由协程的退出逻辑保证)
            isCurrentSoundPlaying = false;
            hasStartedAudioSequence = false;
        }
        else
        {
            

            // 只有当音频未在播放且未被标记为已触发时，才启动一次性音频播放
            if (currentSoundClip != null && !hasStartedAudioSequence)
            {
                // 启动协程进行一次性播放
                StartCoroutine(PlayCurrentSoundAndReset());
            }
        }
    }

    
    private IEnumerator PlayCurrentSoundAndReset()
    {
        if (audioSource == null || currentSoundClip == null)
        {
            hasStartedAudioSequence = false;
            yield break;
        }

       
        hasStartedAudioSequence = true;

     
        isCurrentSoundPlaying = true;

        // 播放音频
        audioSource.PlayOneShot(currentSoundClip);
        Debug.Log("PowerDistributionBox: 播放电流声音频。", this);

        // 等待音频播放完毕
        yield return new WaitForSeconds(currentSoundClip.length);

        
        isCurrentSoundPlaying = false;

      
        hasStartedAudioSequence = false;
    }

    private void Open(bool isOpen)
    {
        if (cover_Close == null || cover_Open == null) return;
        if (isOpen&&!cover_Open.activeSelf)
        {
            cover_Open.SetActive(true);
            cover_Close.SetActive(false);
        }
        else if(!isOpen && !cover_Close.activeSelf)
        {
            cover_Open.SetActive(false);
            cover_Close.SetActive(true);
        }
    }
}
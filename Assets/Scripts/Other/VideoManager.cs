using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    // 单例静态实例
    public static VideoManager Instance { get; private set; }

    [Header("核心组件")]
    [SerializeField] private VideoPlayer videoPlayer; // 拖入场景中的 VideoPlayer 组件
    [SerializeField] private RawImage videoDisplayImage; // 用于在 UI 上显示视频的 RawImage 组件
    [SerializeField] private RenderTexture videoRenderTexture; // 视频的渲染目标

    public VideoClip start;
    public VideoClip electricity;
    public VideoClip findFeet;
    public VideoClip end;
    private VideoClip currentClip;

    // 新增：播放完毕并保持画面的事件，用于通知 UI 显示按钮
    public event Action OnVideoHeld;
    // 播放结束并恢复游戏时间的事件
    public event Action OnVideoFinished;

    public event Action<AnimationCompleteEventArgs> OnVideoComplete;

    private AnimationCompleteEventArgs pendingArgs; // 用于存储 过关检测脚本 传入的参数

    private void Awake()
    {
        // 确保只有一个实例 (单例模式)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景时保留
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 确保组件已连接
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer is not assigned!");
        }

        // 配置 VideoPlayer 的渲染目标
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;

        // 订阅播放结束事件
        videoPlayer.loopPointReached += OnVideoFinishedHandler;

        // 初始状态，隐藏 RawImage
        if (videoDisplayImage != null)
        {
            videoDisplayImage.enabled = false;
        }
    }

    private void Start()
    {
        PlayVideoClip(start,null);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            // 取消订阅
            videoPlayer.loopPointReached -= OnVideoFinishedHandler;
            Instance = null;
        }
    }

    // --- 公开的播放方法 ---

    /// <summary>
    /// 播放一个 VideoClip 资产。
    /// </summary>
    /// <param name="clip">要播放的 VideoClip 文件（MP4 导入 Unity 后生成的资产）。</param>
    /// <param name="loop">是否循环播放。</param>
    public void PlayVideoClip(VideoClip clip, AnimationCompleteEventArgs args, bool loop = false)
    {
        currentClip = clip;
        this.pendingArgs = args;
        if (clip == null)
        {
            Debug.LogError("VideoClip is null. Cannot play video.");
            return;
        }

        // 1. 设置视频源
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;
        videoPlayer.isLooping = loop;

        // 2. 准备和播放
        if (videoDisplayImage != null)
        {
            videoDisplayImage.enabled = true; // 显示视频容器
        }

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnPrepareCompleted;

        Time.timeScale = 0;
    }

    // Resources 方式（不推荐，但提供参考）
    // 您可以根据需要修改此方法，使用 Resources.Load<VideoClip>("文件名") 来加载
    /*
    public void PlayVideoFromResources(string videoFileName) 
    {
         VideoClip clip = Resources.Load<VideoClip>(videoFileName);
         PlayVideoClip(clip);
    }
    */

    // --- 内部辅助方法 ---

    private void OnPrepareCompleted(VideoPlayer source)
    {
        source.prepareCompleted -= OnPrepareCompleted; // 移除一次性事件
        source.Play(); // 准备完成后立即播放
        Debug.Log("Video started playing: " + source.clip.name);
    }

    private void OnVideoFinishedHandler(VideoPlayer source)
    {
        if (!source.isLooping)
        {
            // 停止 VideoPlayer 播放，但不要隐藏画面 (RawImage)
            source.Stop();
    
            Debug.Log("VideoManager: OnVideoFinishedHandler called, triggering OnVideoHeld event");
            
            // 检查是否有事件监听器
            Debug.Log("VideoManager: OnVideoHeld event listeners count: " + (OnVideoHeld != null ? "at least 1" : "0"));
            
            // 触发事件
            OnVideoHeld?.Invoke();
        }
    }

    /// <summary>
    /// 由用户交互按钮调用。清除视频画面并恢复游戏时间。
    /// </summary>
    public void ContinueGameFromCutscene()
    {
        // 隐藏画面
        if (videoDisplayImage != null)
        {
            videoDisplayImage.enabled = false;
        }

        // 恢复游戏时间
        if (Time.timeScale < 1f)
        {
            Time.timeScale = 1f;
            Debug.Log("游戏时间已恢复 (Time.timeScale = 1)。");
        }

        // 触发最终完成事件
        OnVideoFinished?.Invoke();

        if (pendingArgs != null)
        {
            OnVideoComplete?.Invoke(pendingArgs);
        }
        this.pendingArgs = null;


        //if (currentClip == start)
        //{
        //    TutorialManager.instance.StartPrompt();
        //}
    }

    /// <summary>
    /// 停止播放视频并隐藏显示容器。
    /// </summary>
    public void StopVideo()
    {
        videoPlayer.Stop();
        ContinueGameFromCutscene();
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    // 使用单例模式确保全局唯一
    public static SettingsManager Instance;

    // 存储设置数据
    public float GlobalVolume { get; private set; } = 1.0f;
    public Vector2 CurrentResolution { get; private set; }

    // **关键引用：Audio Mixer**
    public AudioMixer masterMixer;

    // UI 控件（用于刷新面板时显示当前值）
    [SerializeField] private Slider volumeSlider;

    private const string MASTER_VOLUME_PARAM = "MasterVolume"; // 匹配 Audio Mixer 中的暴露参数名称
    private const string GLOBAL_VOLUME_KEY = "GlobalVolume"; // PlayerPrefs 键名
    private const string FULLSCREEN_KEY = "IsFullscreen";


    // 引用设置面板的根GameObject
    [SerializeField] private GameObject settingsPanel;

    // UI 控件引用
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle; // 新增对 Toggle 的引用

    // 支持的分辨率列表 (通常使用 16:9 比例)
    // 注意：您也可以使用 Screen.resolutions 来获取设备支持的所有分辨率
    private readonly List<ResolutionInfo> supportedResolutions = new List<ResolutionInfo>
    {
        new ResolutionInfo { width = 1920, height = 1080, label = "1920 x 1080 (16:9)" },
        new ResolutionInfo { width = 1600, height = 900, label = "1600 x 900 (16:9)" },
        new ResolutionInfo { width = 1280, height = 720, label = "1280 x 720 (16:9)" },
        new ResolutionInfo { width = 1024, height = 768, label = "1024 x 768 (4:3)" } // 示例
    };

    private const string RESOLUTION_INDEX_KEY = "ResolutionIndex";

    // 辅助结构体来存储分辨率信息
    [System.Serializable]
    public struct ResolutionInfo
    {
        public int width;
        public int height;
        public string label;
    }

    private void Awake()
    {
        // 实现单例模式：确保只有一个 SettingsManager 实例
        if (Instance == null)
        {
            Instance = this;
            // **关键步骤：场景切换时不销毁此 GameObject**
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            // 如果已经存在实例，则销毁这个新的实例
            Destroy(gameObject);
            return;
        }

        // 首次加载时初始化设置
        LoadSettings();
    }

    private void Start()
    {
        InitializeResolutionDropdown();
    }

    // --- 公共调用方法 ---

    public void OpenPanel()
    {
        // 激活设置面板
        settingsPanel.SetActive(true);
        // 可以在这里暂停游戏：Time.timeScale = 0f;

        // 同步全屏 Toggle
        if (fullscreenToggle != null)
        {
            // 使用当前的游戏状态来更新 UI
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }

    public void ClosePanel()
    {
        // 禁用设置面板
        settingsPanel.SetActive(false);
        // 可以在这里恢复游戏：Time.timeScale = 1f;

        SaveSettings();
    }

    // --- 设置操作方法 ---

    // --- 调整音量的方法 (供 Slider 调用) ---
    public void SetGlobalVolume(float linearVolume) // linearVolume 是滑块的 0 到 1.0f 值
    {
        // 关键转换：将线性值 (0-1) 转换为对数分贝值 (dB)，用于 Audio Mixer
        float dB;
        if (linearVolume <= 0.0001f) // 避免 Log(0) 导致 -Infinity，小于一个极小值就设为最低分贝（静音）
        {
            dB = -80f; // Audio Mixer 中 -80dB 通常被视为静音
        }
        else
        {
            // 公式：20 * log10(线性音量)
            dB = 20f * Mathf.Log10(linearVolume);
        }

        masterMixer.SetFloat(MASTER_VOLUME_PARAM, dB);

        // **确保滑块的 UI 显示与当前音量匹配**
        if (volumeSlider != null && volumeSlider.value != linearVolume)
        {
            // 关键：暂时移除监听器，以防止程序设置 value 再次触发 SetGlobalVolume 形成循环
            volumeSlider.onValueChanged.RemoveListener(SetGlobalVolume);

            volumeSlider.value = linearVolume;

            // 重新添加监听器
            volumeSlider.onValueChanged.AddListener(SetGlobalVolume);
        }

        // **同时保存线性值到本地，方便下次启动和更新滑块**
        PlayerPrefs.SetFloat(GLOBAL_VOLUME_KEY, linearVolume);
    }

    // 供分辨率下拉菜单调用
    public void SetResolution(int width, int height)
    {
        // 设置分辨率并应用全屏模式（可选）
        Screen.SetResolution(width, height, Screen.fullScreen);
        CurrentResolution = new Vector2(width, height);
    }

    // --- 数据持久化方法 ---
    // --- 数据加载方法 ---
    private void LoadSettings()
    {
        // 1. 加载音量 (已完成)
        float savedVolume = PlayerPrefs.GetFloat(GLOBAL_VOLUME_KEY, 1.0f);
        SetGlobalVolume(savedVolume);

        // 2. 加载分辨率
        int savedResIndex = PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY, 0);
        if (savedResIndex >= 0 && savedResIndex < supportedResolutions.Count)
        {
            ResolutionInfo res = supportedResolutions[savedResIndex];
            // 关键：在启动时应用保存的分辨率和全屏模式
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }

        // 3. 加载全屏模式
        bool isFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1) == 1; // 默认全屏
        Screen.fullScreen = isFullscreen;
    }

    private void SaveSettings()
    {
        // 示例：保存音量
        PlayerPrefs.SetFloat("GlobalVolume", GlobalVolume);
        PlayerPrefs.Save();

        // 示例：保存分辨率
        // ...
    }

    private void InitializeResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        // 1. 填充 Dropdown 选项文本
        foreach (var res in supportedResolutions)
        {
            options.Add(res.label);
        }
        resolutionDropdown.AddOptions(options);

        // 2. 找到保存的索引
        int savedIndex = PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY, 0);

        // 3. 设置 Dropdown 的显示值
        if (savedIndex >= 0 && savedIndex < supportedResolutions.Count)
        {
            resolutionDropdown.value = savedIndex;
            resolutionDropdown.RefreshShownValue();
        }

        // 4. 绑定事件 (重要! 确保 SetResolutionByIndex 每次都被调用)
        // 假设您使用 Inspector 绑定，此处可以省略 AddListener。
        // 如果使用代码绑定，则添加：
        // resolutionDropdown.onValueChanged.AddListener(SetResolutionByIndex);
    }


    // 供分辨率下拉菜单调用（绑定到 On Value Changed）
    public void SetResolutionByIndex(int resolutionIndex)
    {
        if (resolutionIndex >= 0 && resolutionIndex < supportedResolutions.Count)
        {
            ResolutionInfo selectedResolution = supportedResolutions[resolutionIndex];

            // 应用分辨率。使用当前的全屏状态或默认全屏。
            // Screen.fullScreenMode 更加灵活，例如 FullScreenMode.ExclusiveFullScreen
            Screen.SetResolution(selectedResolution.width,
                                 selectedResolution.height,
                                 Screen.fullScreen);

            // 保存选中的索引
            PlayerPrefs.SetInt(RESOLUTION_INDEX_KEY, resolutionIndex);
            PlayerPrefs.Save();

            // 更新内部状态 (可选)
            CurrentResolution = new Vector2(selectedResolution.width, selectedResolution.height);
        }
    }

    // 供 Toggle 调用（绑定到 On Value Changed (Boolean)）
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        // 保存全屏状态 (true 存 1, false 存 0)
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}

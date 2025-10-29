using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;
    // === UI 引用 ===
    public TMP_Text goalText;
    public GameObject tabUIIndicator; // 左上角的 Tab UI
    public TextMeshProUGUI tabPromptText; // 新增：用于 "按TAB查看提示" 的文本
    public TextMeshProUGUI tabMenuContentText;
    public GameObject tabMenuPanel;     // TAB 菜单面板 (3.1)
    public TextMeshProUGUI wasdPromptText; // WASD 提示文本 (3.2)
    public float initialDelay = 3f; // 初始停留时间
    public float fadeOutDuration = 0.4f; // 渐隐持续时间
    public float moveDuration = 1.0f; // 移动到左上角的时间

    public GameObject nextButton; // 用于播放完动画后的下一步

    private Vector3 centerPosition;
    private Vector3 tabUIPosition;

    // 存储初始的 WASD 文本颜色，以便隐藏时设置其Alpha为0
    private Color initialWASDColor;

    private void Awake()
    {
        // 单例模式
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 确保一开始屏幕上无其他 UI
        if (tabUIIndicator != null)
        {
            tabUIIndicator.SetActive(false);
        }

        // 记录初始中心位置（GoalText的RectTransform的本地位置）
        centerPosition = goalText.rectTransform.localPosition;

        // 记录目标位置：TabUIIndicator的位置
        // 注意：如果 GoalText 和 TabUIIndicator 的父级（Canvas）设置不同，
        // 可能需要转换坐标系。这里假设它们都在同一个 Canvas 下。
        if (tabUIIndicator != null)
        {
            // 获取 TabUIIndicator 的世界坐标，并将其转换回 GoalText 所在的本地坐标系
            Vector3 worldPos = tabUIIndicator.transform.position;
            tabUIPosition = goalText.rectTransform.parent.InverseTransformPoint(worldPos);

            // 稍微偏移一下，让文字看起来是飞到UI旁边而不是中心点
            // 修改：增加y轴偏移，让文字飞的终点更靠上
            tabUIPosition += new Vector3(goalText.rectTransform.rect.width / 2, 55f, 0); // 50f可以根据需要调整
        }

        //// 启动引导流程的第一个步骤
        //StartCoroutine(Step1_InitialGoalPrompt());

        VideoManager.Instance.OnVideoHeld += ActiveNextButton;
        VideoManager.Instance.OnVideoFinished += DeactivateNextButton;
        VideoManager.Instance.OnVideoFinished += StartPrompt;
        VideoManager.Instance.OnVideoComplete += ConfigureAndStartSimplifiedPrompt;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (tabMenuPanel.activeSelf)
            {
                // 原来是直接隐藏，现在改为启动渐隐动画协程
                StartCoroutine(FadeOutTabMenu());
            }
            else
            {
                // 显示 TAB 菜单
                tabMenuPanel.SetActive(true);
                // 确保开始时是完全不透明的
                SetPanelOpacity(tabMenuPanel, 1f);
                
                // 可选：启动自动渐隐计时器
                StartCoroutine(AutoHideTabMenu());
            }
        }
    }
    
    // 新增：Tab菜单渐隐动画协程
    private IEnumerator FadeOutTabMenu()
    {
        float fadeDuration = 0.4f; // 渐隐持续时间，可根据需要调整
        float timer = 0f;
        
        // 记录开始时的透明度（假设是完全不透明）
        float startOpacity = 1f;
        
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / fadeDuration;
            float currentOpacity = Mathf.Lerp(startOpacity, 0f, normalizedTime);
            
            // 设置面板透明度
            SetPanelOpacity(tabMenuPanel, currentOpacity);
            
            yield return null;
        }
        
        // 确保最终状态
        tabMenuPanel.SetActive(false);
        // 重置透明度，以便下次显示时是完全不透明的
        SetPanelOpacity(tabMenuPanel, 1f);
    }
    
    // 新增：自动隐藏Tab菜单的协程（可选功能）
    private IEnumerator AutoHideTabMenu()
    {
        float autoHideDelay = 3f; // 显示多长时间后自动开始渐隐，可根据需要调整
        yield return new WaitForSeconds(autoHideDelay);
        
        // 确保在开始渐隐前菜单仍然是激活的
        if (tabMenuPanel.activeSelf)
        {
            StartCoroutine(FadeOutTabMenu());
        }
    }
    
    // 新增：设置UI面板及其所有子元素透明度的辅助方法
    private void SetPanelOpacity(GameObject panel, float opacity)
    {
        // 设置面板自身的透明度
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = opacity;
    }

    void ActiveNextButton()
    {
        nextButton.SetActive(true);
    }
    void DeactivateNextButton()
    {
        nextButton.SetActive(false);
    }

    // 新增：处理nextButton点击事件
    // public void OnNextButtonClick()
    // {
    //     // 立即将按钮透明度设为0
    //     CanvasGroup canvasGroup = nextButton.GetComponent<CanvasGroup>();
    //     if (canvasGroup == null)
    //     {
    //         canvasGroup = nextButton.AddComponent<CanvasGroup>();
    //     }
    //     canvasGroup.alpha = 0f;
        
    //     // 启动协程，延迟1秒后失活按钮
    //     StartCoroutine(DeactivateButtonAfterDelay(1f));
    // }

    // // 新增：延迟失活按钮的协程
    // private IEnumerator DeactivateButtonAfterDelay(float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //     nextButton.SetActive(false);
    // }

    public void StartPrompt()
    {
        // 启动引导流程的第一个步骤
        StartCoroutine(Step1_InitialGoalPrompt());
    }
    IEnumerator Step1_InitialGoalPrompt()
    {
        // 1.2 目标提示：中央文字浮现（假设 GoalText 初始是可见的）
        // 也可以加一个淡入效果
        goalText.gameObject.SetActive(true);

        // 1.2 停留3秒
        yield return new WaitForSeconds(initialDelay);

        // 1.3 左上角"tab"UI出现
        if (tabUIIndicator != null)
        {
            tabUIIndicator.SetActive(true);
        }

        // 1.3 移动到左上角并渐隐

        // -----------------------------------------------------
        // 动画实现：同时进行移动和渐隐
        // -----------------------------------------------------

        float timer = 0f;
        Color initialColor = goalText.color;

        while (timer < moveDuration) // 使用 moveDuration 作为动画总时长
        {
            // 计算插值
            float t = timer / moveDuration;

            // 移动：使用 Lerp 实现平滑移动
            goalText.rectTransform.localPosition = Vector3.Lerp(centerPosition, tabUIPosition, t);

            // 渐隐（在移动的最后 0.5 秒开始）
            if (timer >= moveDuration - fadeOutDuration)
            {
                float fadeT = (timer - (moveDuration - fadeOutDuration)) / fadeOutDuration;
                // 确保 t 在 [0, 1] 范围内
                fadeT = Mathf.Clamp01(fadeT);

                // 改变颜色 alpha
                Color newColor = initialColor;
                newColor.a = Mathf.Lerp(1f, 0f, fadeT);
                goalText.color = newColor;
            }

            timer += Time.deltaTime;
            yield return null; // 等待下一帧
        }

        // 确保最终状态
        goalText.gameObject.SetActive(false); // 最终消失

        // 流程进入下一步
        StartCoroutine(Step2_TabCheckPrompt());
    }

    // Step 2: TAB操作提示
    IEnumerator Step2_TabCheckPrompt()
    {
        // 确保一开始是透明的
        Color initialColor = tabPromptText.color;
        initialColor.a = 0f;
        tabPromptText.color = initialColor;
        tabPromptText.gameObject.SetActive(true);

        // 2.1 & 2.2 0.5秒渐现
        float fadeInTimer = 0f;
        float fadeInDuration = 0.5f;

        while (fadeInTimer < fadeInDuration)
        {
            fadeInTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, fadeInTimer / fadeInDuration);
            Color c = initialColor;
            c.a = alpha;
            tabPromptText.color = c;
            yield return null;
        }
        // 确保完全显示
        initialColor.a = 1f;
        tabPromptText.color = initialColor;

        // 2.2 停留3秒
        yield return new WaitForSeconds(1f);

        // 2.2 0.5秒渐隐
        float fadeOutTimer = 0f;
        float fadeOutDuration = 0.5f;

        while (fadeOutTimer < fadeOutDuration)
        {
            fadeOutTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeOutTimer / fadeOutDuration);
            Color c = initialColor;
            c.a = alpha;
            tabPromptText.color = c;
            yield return null;
        }

        // 最终消失
        tabPromptText.gameObject.SetActive(false);

        // 流程进入下一步：检测玩家输入
        StartCoroutine(Step3_InputCheckAndFlow());
    }

    // Step 3: 玩家输入判断 (参考图: image_4cbf62.png, image_4cbf49.png, image_4cbf43.png)
    IEnumerator Step3_InputCheckAndFlow()
    {
        // --- 确保 TAB 提示文本已经消失或足够长时间未按下 ---
        // 持续检测 TAB 键
        while (true)
        {
            //Debug.Log("开始检测");
            //// === 3.1 按下tab键流程 ===
            //if (Input.GetKeyDown(KeyCode.Tab))
            //{
            //    tabUIIndicator.SetActive(false);
            //    // 暂停游戏（可选）
            //    //Time.timeScale = 0f;

            //    // 显示 TAB 菜单
            //    tabMenuPanel.SetActive(true);

            //    // 避免第一次 GetKeyDown 触发后，立即进入下一次 GetKeyDown 检测
            //    yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Tab));

            //    // 此时只等待 GetKeyDown，不会被上一个按键信号干扰
            //    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Tab));

            //    // 隐藏 TAB 菜单
            //    tabMenuPanel.SetActive(false);

            //    // 恢复游戏
            //    //Time.timeScale = 1f;

            //    // 退出此循环，进入 WASD 流程 (流程切换)
            //    break;
            //}
            // 如果 Step 2 结束时玩家没有按下 TAB，我们不能让它一直循环，
            // 否则会阻塞后续流程。
            // 由于 Step 2 的 TAB 提示已经消失，我们应该继续 WASD 流程。

            // 修正逻辑：我们不应该在这里无限循环等待 TAB，而应该给一个短时间等待，
            // 然后进入 WASD 流程，让玩家在游戏过程中随时按 TAB。
            // 然而，根据您的描述，TAB 菜单的流程是**一次性引导**的一部分。

            // 如果我们假设 Step 3 是在 Step 2 结束后立即执行，
            // 并且我们给玩家 3 秒的额外时间来按 TAB，我们可以这样写：

            float tabCheckTimer = 3f; // 额外给 3 秒时间反应
            while (tabCheckTimer > 0f)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    // 进入 TAB 菜单流程 (同上，为了避免重复代码，可以封装成方法)
                    yield return StartCoroutine(HandleTabMenu());
                    // 退出主流程循环
                    goto EndTabCheck;
                }
                else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    // 玩家按下了 WASD 键，直接跳过 TAB 流程，进入 WASD 流程
                    yield break;
                }
                tabCheckTimer -= Time.deltaTime;
                yield return null;
            }

            // 如果计时器结束，跳出循环
            break;
        }

    EndTabCheck:
        // 无论是否按了 TAB，流程都将继续执行 WASD 提示 (3.2 不按tab键/退出后)
        StartCoroutine(Step3_2_WASDPrompt());
    }

    IEnumerator HandleTabMenu()
    {
        // 显示 TAB 菜单
        tabMenuPanel.SetActive(true);

        // 等待 TAB 键释放
        yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Tab));

        //等待玩家再次按下 TAB 退出
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Tab));

        // 隐藏 TAB 菜单
        tabMenuPanel.SetActive(false);

        // 3：等待 TAB 键再次释放
        // 确保退出菜单后，不会立即重新进入 TAB 检测
        yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Tab));
    }


    // Step 3.2: WASD 提示与隐藏逻辑 (参考图: image_4cbf43.png)
    IEnumerator Step3_2_WASDPrompt()
    {
        // 3.2.1 屏幕顶部出现：WASD移动
        wasdPromptText.gameObject.SetActive(true);

        // 持续检测 WASD 任意键
        while (true)
        {
            // 检查 WASD 任意键是否被按下
                 bool wasdPressed = Input.GetKeyDown(KeyCode.W) ||
                               Input.GetKeyDown(KeyCode.A) ||
                               Input.GetKeyDown(KeyCode.S) ||
                               Input.GetKeyDown(KeyCode.D);

            if (wasdPressed)
            {
                // 玩家按键，执行 1 秒后渐隐 0.5 秒消失
                yield return new WaitForSeconds(1.0f);

                float fadeOutTimer = 0f;
                float fadeOutDuration = 0.5f;
                Color initialColor = wasdPromptText.color;

                // 渐隐 0.5 秒
                while (fadeOutTimer < fadeOutDuration)
                {
                    fadeOutTimer += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, fadeOutTimer / fadeOutDuration);
                    Color c = initialColor;
                    c.a = alpha;
                    wasdPromptText.color = c;
                    yield return null;
                }

                // 最终消失
                wasdPromptText.gameObject.SetActive(false);

                // 引导流程结束
                break;
            }

            // 3.2.2 如果不按 WASD，就永久不消失
            yield return null; // 每帧检查
        }
    }


    /// <summary>
    /// 配置引导文本内容，并启动简化的引导流程。
    /// 流程：GoalPrompt (移动/渐隐) -> TabPrompt (渐入/渐出) -> TabInputCheck (结束)
    /// </summary>
    /// <param name="newGoalText">新的目标提示文本内容。</param>
    /// <param name="newTabPrompt">新的TAB提示文本内容。</param>
    /// <summary>
     /// 配置引导文本内容（GoalText 和 Tab 菜单内部文本），并启动简化的引导流程。
     /// 流程：GoalPrompt (移动/渐隐) -> TabPrompt (渐入/渐出) -> InputCheck (结束)
     /// </summary>
     /// <param name="newGoalText">新的目标提示文本内容。</param>
     /// <param name="newTabMenuContent">TAB 菜单内部需要修改的文本内容。</param>
     /// <param name="newTabPrompt">（可选）新的TAB提示文本内容。如果不提供，使用默认值。</param>
    public void ConfigureAndStartSimplifiedPrompt(AnimationCompleteEventArgs args)
    {
        // 1. 停止所有正在运行的引导协程，防止干扰
        StopAllCoroutines();

        // 2. 更新 UI 内容
        if (goalText != null)
        {
            goalText.text =args.newGoalText;
            // 确保 goalText 在开始前是可见且不透明的
            Color c = goalText.color;
            c.a = 1f;
            goalText.color = c;
        }

        // 更新 TAB 菜单内部的文本 <--- 重点修改部分
        if (tabMenuContentText != null)
        {
            tabMenuContentText.text = args.newTabMenuContent;
        }

        // 3. 确保 WASD 提示绝对隐藏（重构要求）
        if (wasdPromptText != null)
        {
            wasdPromptText.gameObject.SetActive(false);
            wasdPromptText.color = initialWASDColor; // 恢复颜色以便下次使用
        }

        // 4. 启动简化的流程
        StartCoroutine(Step_SimplifiedGoalPrompt());
    }

    // =========================================================
    // 【简化流程协程 - 逻辑不变，只更新注释和连接】
    // =========================================================

    IEnumerator Step_SimplifiedGoalPrompt()
    {
        // 1.2 目标提示：中央文字浮现
        goalText.gameObject.SetActive(true);
        yield return new WaitForSeconds(initialDelay);

        // 1.3 左上角"tab"UI出现
        if (tabUIIndicator != null)
        {
            tabUIIndicator.SetActive(true);
        }

        // 1.3 移动到左上角并渐隐 (动画逻辑不变)
        float timer = 0f;
        Color initialColor = goalText.color;

        while (timer < moveDuration)
        {
            float t = timer / moveDuration;

            // 移动
            goalText.rectTransform.localPosition = Vector3.Lerp(centerPosition, tabUIPosition, t);

            // 渐隐
            if (timer >= moveDuration - fadeOutDuration)
            {
                float fadeT = (timer - (moveDuration - fadeOutDuration)) / fadeOutDuration;
                fadeT = Mathf.Clamp01(fadeT);

                Color newColor = initialColor;
                newColor.a = Mathf.Lerp(1f, 0f, fadeT);
                goalText.color = newColor;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 确保最终状态
        goalText.gameObject.SetActive(false);

        // 流程进入下一步：【连接到简化的 TAB 提示】
        StartCoroutine(Step_SimplifiedTabCheckPrompt());
    }

    IEnumerator Step_SimplifiedTabCheckPrompt()
    {
        // 确保一开始是透明的
        Color initialColor = tabPromptText.color;
        initialColor.a = 0f;
        tabPromptText.color = initialColor;
        tabPromptText.gameObject.SetActive(true);

        // 渐现
        float fadeInTimer = 0f;
        float fadeInDuration = 0.5f;

        while (fadeInTimer < fadeInDuration)
        {
            fadeInTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, fadeInTimer / fadeInDuration);
            Color c = initialColor;
            c.a = alpha;
            tabPromptText.color = c;
            yield return null;
        }
        initialColor.a = 1f;
        tabPromptText.color = initialColor;

        // 停留
        yield return new WaitForSeconds(1f);

        // 渐隐
        float fadeOutTimer = 0f;
        float fadeOutDuration = 0.5f;

        while (fadeOutTimer < fadeOutDuration)
        {
            fadeOutTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeOutTimer / fadeOutDuration);
            Color c = initialColor;
            c.a = alpha;
            tabPromptText.color = c;
            yield return null;
        }

        // 最终消失
        tabPromptText.gameObject.SetActive(false);

        // 流程进入下一步：【连接到简化的输入检测】
        StartCoroutine(Step_SimplifiedInputCheck());
    }

    IEnumerator Step_SimplifiedInputCheck()
    {
        // 玩家可能在 Step2 结束前就已经按下了 TAB，这里给一个短时间等待（3秒），
        // 允许玩家触发 TAB 菜单。
        float tabCheckTimer = 3f;

        while (tabCheckTimer > 0f)
        {
            // 注意：由于 Update 中有永久的 TAB 菜单开关逻辑，
            // 这里的检测主要是为了给流程一个结束点。

            // 如果玩家按下了 TAB 键，则认为完成了引导的目标，流程可以结束。
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                // 注意：我们不再调用 HandleTabMenu，因为 Update 已经在处理 TAB 菜单的开关。
                // 如果我们希望流程只在玩家第一次按 TAB 时结束，可以设置一个标志位。
                // 在当前简化的需求下，即使玩家不按，流程也应该继续结束。

                // 如果需要确保在流程中至少“确认”一次TAB，可以加入一个短暂的等待。
                yield return null; // 等待一帧，确保 Update 已经处理了 GetKeyDown
                break;
            }
            tabCheckTimer -= Time.deltaTime;
            yield return null;
        }

        // 无论是否按下 TAB，计时器结束后，引导流程都结束。
        Debug.Log("简化的引导流程结束。");
    }

    // 原始的 HandleTabMenu 协程现在不再被简化流程调用，
    // 因为 Update 函数已经接管了菜单的永久开关功能。
    // 如果需要更复杂的流程控制（例如只有在引导期间才能打开一次菜单），
    // 则需要修改 Update 函数或重新引入此协程。
    // 为了简化和满足您“跟原来的流程一样，激活tabMenuPanel”的要求，我们依赖 Update 即可。

    // 原始的 StartPrompt (完整流程) 保持不变，但为了简洁已删除。

}

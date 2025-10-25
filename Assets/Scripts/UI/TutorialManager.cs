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
    public GameObject tabMenuPanel;     // TAB 菜单面板 (3.1)
    public TextMeshProUGUI wasdPromptText; // WASD 提示文本 (3.2)
    public float initialDelay = 3f; // 初始停留时间
    public float fadeOutDuration = 0.5f; // 渐隐持续时间
    public float moveDuration = 1.0f; // 移动到左上角的时间

    public GameObject nextButton; // 用于播放完动画后的下一步

    private Vector3 centerPosition;
    private Vector3 tabUIPosition;

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
            tabUIPosition += new Vector3(goalText.rectTransform.rect.width / 2, 0, 0);
        }

        //// 启动引导流程的第一个步骤
        //StartCoroutine(Step1_InitialGoalPrompt());

        VideoManager.Instance.OnVideoHeld += ActiveNextButton;
        VideoManager.Instance.OnVideoFinished += DeactivateNextButton;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (tabMenuPanel.activeSelf)
            {
                // 隐藏 TAB 菜单
                tabMenuPanel.SetActive(false);
            }
            else
            {
                // 显示 TAB 菜单
                tabMenuPanel.SetActive(true);
            }
        }
    }

    void ActiveNextButton()
    {
        nextButton.SetActive(true);
    }
    void DeactivateNextButton()
    {
        nextButton.SetActive(false);
    }

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
}

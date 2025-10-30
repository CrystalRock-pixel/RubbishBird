using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource audioSource;
    private AudioClip currentClip;
    public float fadeDuration = 1.5f; // 淡入淡出时长

    private Coroutine fadeCoroutine;

    // 用于处理嵌套场景的优先级队列
    private System.Collections.Generic.List<MusicZone> activeZones = new System.Collections.Generic.List<MusicZone>();

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 进入一个音乐区域
    public void EnterZone(MusicZone zone)
    {
        if (!activeZones.Contains(zone))
        {
            activeZones.Add(zone);
            UpdateMusic();
        }
    }

    // 离开一个音乐区域
    public void ExitZone(MusicZone zone)
    {
        if (activeZones.Contains(zone))
        {
            activeZones.Remove(zone);
            UpdateMusic();
        }
    }

    // 根据当前活跃的区域更新音乐
    private void UpdateMusic()
    {
        if (activeZones.Count == 0)
        {
            // 没有任何区域，停止音乐
            StopMusic();
            return;
        }

        // 找出优先级最高的区域
        MusicZone highestPriorityZone = activeZones[0];
        foreach (var zone in activeZones)
        {
            if (zone.priority > highestPriorityZone.priority)
            {
                highestPriorityZone = zone;
            }
        }

        // 播放优先级最高区域的音乐
        ChangeMusic(highestPriorityZone.zoneMusic);
    }

    public void ChangeMusic(AudioClip newClip)
    {
        // 如果已经在播放这首音乐，不做处理
        if (currentClip == newClip && audioSource.isPlaying)
        {
            return;
        }

        // 停止之前的淡入淡出协程
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeAndChangeMusic(newClip));
    }

    private IEnumerator FadeAndChangeMusic(AudioClip newClip)
    {
        // 淡出当前音乐
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();

        // 切换音乐
        currentClip = newClip;
        audioSource.clip = newClip;
        audioSource.Play();

        // 淡入新音乐
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 1;
    }

    public void StopMusic()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 1;
    }
}
    


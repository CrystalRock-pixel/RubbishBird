using UnityEngine;
public class MoveMouse : MonoBehaviour
{
    public RectTransform gameName;
    public RectTransform character;
    [Range(0f, 1f)]
    public float moveAmount = 0.05f;
    [Range(0f, 1f)]
    public float characterMoveLimit = 0.3f;  

    private Vector2 screenCenter;
    private Vector2 gameNameStart;
    private Vector2 characterStart;

    void Start()
    {
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        gameNameStart = gameName.anchoredPosition;
        characterStart = character.anchoredPosition;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 offset = (mousePos - screenCenter) / screenCenter;
        offset *= moveAmount * Screen.width;


        Vector2 targetGameName = gameNameStart + offset;
        gameName.anchoredPosition = Vector2.Lerp(gameName.anchoredPosition, targetGameName, 5f * Time.deltaTime);

    
        Vector2 limitedOffset = offset * 1.2f;
        // 限制最大移动距离
        if (limitedOffset.magnitude > characterMoveLimit * Screen.width * moveAmount)
        {
            limitedOffset = limitedOffset.normalized * characterMoveLimit * Screen.width * moveAmount;
        }

        Vector2 targetCharacter = characterStart + limitedOffset;
        character.anchoredPosition = Vector2.Lerp(character.anchoredPosition, targetCharacter, 5f * Time.deltaTime);
    }
}
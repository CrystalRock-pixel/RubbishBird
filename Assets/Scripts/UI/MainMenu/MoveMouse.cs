
using UnityEngine;

public class MoveMouse : MonoBehaviour
{
    public RectTransform gameName;
    public RectTransform character;

    [Range(0f, 1f)]
    public float moveAmount = 0.05f;
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
        Vector2 targetCharacter = characterStart + offset * 1.2f;

        gameName.anchoredPosition = Vector2.Lerp(gameName.anchoredPosition, targetGameName, 5f * Time.deltaTime);
        character.anchoredPosition = Vector2.Lerp(character.anchoredPosition, targetCharacter, 5f * Time.deltaTime);
    }
}

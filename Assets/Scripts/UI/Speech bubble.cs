using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Speechbubble : MonoBehaviour
{
    public Image image;
    public SpriteRenderer spriteRenderer;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Transform canvas = transform.GetChild(0);
        Transform Image = canvas.GetChild(0);
        this.image = Image.GetComponent<Image>();
    }

    public void SetSprite(Sprite sprite)
    {
        if (sprite == null) return;
        image.sprite = sprite;
    }

    public void SetAlpha(float alpha)
    {
        spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
        image.color = new Color(1f, 1f, 1f, alpha);
    }
}

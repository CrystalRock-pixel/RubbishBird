using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextDialog : MonoBehaviour
{
    public TMP_Text text;
    private void Start()
    {
        text=transform.GetChild(0).transform.GetComponent<TMP_Text>();
    }

    public void SetText(string _text)
    {
        text.text = _text;
    }
}

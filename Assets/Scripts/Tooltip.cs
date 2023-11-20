using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//Source: https://www.youtube.com/watch?v=HXFoUGw7eKk
//[ExecuteInEditMode()]
public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI contentField;
    public LayoutElement layoutElement;
    public int characterWrapLimit;
    public RectTransform rectTransform;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetText(string content) {
        contentField.text = content;
    }

    private void Update() {
        int contentLength = contentField.text.Length;

        layoutElement.enabled = contentLength > characterWrapLimit;

        Vector2 position = Input.mousePosition;

        float pivotX = position.x / Screen.width;
        float pivotY = position.y / Screen.height;


        rectTransform.pivot = new Vector2(pivotX, pivotY);
        transform.position = position;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple Script for following the mouse.
public class MouseFollow : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    // Update is called once per frame
    void Update()
    {
        rectTransform.position = Input.mousePosition;
    }
}

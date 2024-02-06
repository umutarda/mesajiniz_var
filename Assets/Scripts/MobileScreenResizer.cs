using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileScreenResizer : MonoBehaviour
{
    void Start()
    {   
        RectTransform rectTransform = GetComponent<RectTransform>();
        float multiplier = Screen.width/(FindObjectOfType<Canvas>().scaleFactor * rectTransform.rect.width);
        transform.localScale *= multiplier;
    }


}

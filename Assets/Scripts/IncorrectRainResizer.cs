using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncorrectRainResizer : MonoBehaviour
{
    private static readonly int REFERENCE_RESOLUTION = 1920;
    [SerializeField] private float baseSize = .75f;
    private ParticleSystem incorrectRain;
    

    private void Start()
    {
        incorrectRain = transform.GetChild(0).GetComponent<ParticleSystem>();
        var resized = incorrectRain.shape;
        resized.radius = (baseSize * REFERENCE_RESOLUTION) / Screen.width;
    }

}

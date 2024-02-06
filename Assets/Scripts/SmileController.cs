using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmileController : MonoBehaviour
{
    [SerializeField] private Sprite[] idle1Animation;
    [SerializeField] private Sprite[] idle2Animation;
    [SerializeField] private Sprite[] lose1Animation;
    [SerializeField] private Sprite[] lose2Animation;
    [SerializeField] private Sprite[] tripAnimation;
    [SerializeField] private Sprite[] win1Animation;
    [SerializeField] private Sprite[] win2Animation;

    [SerializeField] private float tripThreshold;

   [SerializeField] private int coolWinCorrectAnswerThreshold;

    private IEnumerator currentRoutine;
    private WaitForSecondsRealtime nextFrameWFS;
    private Image smileImage;
    private bool animationLoop = true;
    private string animationPlayed;
    private const int FPS = 12;

    void Awake()
    {
        smileImage = GetComponent<Image>();
        nextFrameWFS = new WaitForSecondsRealtime(1.0F/FPS);
        animationPlayed = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator PlayAnimation(Sprite[] animation) 
    {
        int index = 0;

        while (index < animation.Length) 
        {
                smileImage.sprite = animation[index];
                index++;

                yield return nextFrameWFS;
            
            
        }

        if (animationLoop) 
        {
            StartCoroutine(currentRoutine);
        }

    }

    public void KillAnimation() 
    {
        StopCoroutine(currentRoutine);
    }

    public void LoopAnimation() 
    {
        animationLoop = true;
    }

    public void PlayAnimation(string name) 
    {
         if(currentRoutine != null)
            StopCoroutine(currentRoutine);
        switch(name) 
        {
            case "idle1": currentRoutine = (PlayAnimation(idle1Animation)); break;
            case "idle2": currentRoutine = (PlayAnimation(idle2Animation)); break;
            case "lose1": currentRoutine = (PlayAnimation(lose1Animation)); break;
            case "lose2": currentRoutine = (PlayAnimation(lose2Animation)); break;
            case "trip": currentRoutine = (PlayAnimation(tripAnimation)); break;
            case "win1": currentRoutine = (PlayAnimation(win1Animation)); break;
            case "win2": currentRoutine = (PlayAnimation(win2Animation)); break;
        }

       StartCoroutine(currentRoutine);

        animationPlayed = name;
       
    }

    public float GetTripThreshold() 
    {
        return tripThreshold;
    }

    public int GetCoolWinCorrectAnswerThreshold() 
    {
        return coolWinCorrectAnswerThreshold;
    }
    public string GetAnimationPlayedName() 
    {
        return animationPlayed;
    }
}

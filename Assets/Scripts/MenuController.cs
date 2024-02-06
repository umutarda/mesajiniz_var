using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class MenuController : MonoBehaviour
{
    [SerializeField] private RectTransform titleRectT;
    [SerializeField] private RectTransform playButtonRectT;
    [SerializeField] private RectTransform frameRectT;
    [SerializeField] private Vector2 titleStartPosition;
    [SerializeField] private Vector2 playButtonStartPosition;
    [SerializeField] private Vector2 frameStartPosition;
    [SerializeField] private float frameShakeStrength; 
    [SerializeField] private int frameShakeVibrato; 
    [SerializeField] private float startWaitDuration;
    [SerializeField] private float titleAnimationDuration;
    [SerializeField] private float playButtonAnimationDuration;
    [SerializeField] private float frameAnimationDuration;
    [SerializeField] private float buttonShakeDuration; 
    [SerializeField] private float buttonShakeStrength; 
    [SerializeField] private int buttonShakeVibrato; 

    private Vector2 titleTargetPosition, playButtonTargetPosition, frameTargetPosition;
    private WaitForSecondsRealtime titleWFS, playButtonWFS, startWaitWFS, frameWFS, buttonShakeWFS;
    private EventTrigger buttonTrigger;
    private const Ease TITLE_EASE = Ease.InOutSine;
    private const Ease PLAY_BUTTON_EASE = Ease.InOutSine;
    private const Ease FRAME_EASE = Ease.InOutSine;
    Tween buttonShake;

    private void Awake() 
    {
        titleTargetPosition = titleRectT.anchoredPosition;
        titleRectT.anchoredPosition = titleStartPosition;
        titleRectT.gameObject.SetActive(false);

        playButtonTargetPosition = playButtonRectT.anchoredPosition;
        playButtonRectT.anchoredPosition = playButtonStartPosition;
        playButtonRectT.gameObject.SetActive(false);

        if (frameRectT != null) 
        {
            frameTargetPosition = frameRectT.anchoredPosition;
            frameRectT.anchoredPosition = frameStartPosition;
        }
        
        

        startWaitWFS = new WaitForSecondsRealtime(startWaitDuration);
        titleWFS = new WaitForSecondsRealtime(titleAnimationDuration);
        playButtonWFS = new WaitForSecondsRealtime(playButtonAnimationDuration);
        frameWFS = new WaitForSecondsRealtime(frameAnimationDuration);
        buttonShakeWFS = new WaitForSecondsRealtime(buttonShakeDuration);

        buttonShake = playButtonRectT.transform.DOShakeScale(buttonShakeDuration,buttonShakeStrength,buttonShakeVibrato).SetLoops(-1);
        DOTween.Pause(playButtonRectT.transform);

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) => 
        DOTween.Play(playButtonRectT.transform));

        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((eventData) => 
        DOTween.Pause(playButtonRectT.transform));

        EventTrigger.Entry pointerClick = new EventTrigger.Entry();
        pointerClick.eventID = EventTriggerType.PointerClick;
        pointerClick.callback.AddListener((eventData) => 
        LoadGame());

        buttonTrigger = playButtonRectT.GetComponent<EventTrigger>();
        buttonTrigger.triggers.Add(pointerEnter);
        buttonTrigger.triggers.Add(pointerExit);
        buttonTrigger.triggers.Add(pointerClick);
    }

    void Start()
    {
        StartCoroutine(Opening());
    }

    private IEnumerator Opening() 
    {
        yield return startWaitWFS;

        if (frameRectT != null) 
        {
            frameRectT.DOAnchorPos(frameTargetPosition,frameAnimationDuration).SetEase(FRAME_EASE);
            frameRectT.transform.DOShakeRotation(frameAnimationDuration,frameShakeStrength,frameShakeVibrato);
            yield return frameWFS;
        }
       
        titleRectT.gameObject.SetActive(true);
        titleRectT.DOAnchorPos(titleTargetPosition,titleAnimationDuration).SetEase(TITLE_EASE);
        yield return titleWFS;
        playButtonRectT.gameObject.SetActive(true);
        playButtonRectT.DOAnchorPos(playButtonTargetPosition,playButtonAnimationDuration).SetEase(PLAY_BUTTON_EASE);
        yield return playButtonWFS;
    }

    private void LoadGame() 
    {
        buttonShake.Kill();
        SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex +1);
    }

}

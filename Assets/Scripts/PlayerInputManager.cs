using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.PlayerLoop;

public class PlayerInputManager : MonoBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private Transform textChoices;
    [SerializeField] private Transform imageChoices;
    [SerializeField] private Transform audioChoices;
    [SerializeField] private Image cursorIcon;
    [SerializeField] private AudioSource audioSpeaker;
    [SerializeField] private UIParticle clickCircle;
    //[SerializeField] private UIParticle soundPlaying;

    [Header("Animation Parameters")]
    [SerializeField] private float cursorSmoothEventAnimationDuration;
    [SerializeField] private float cursorFastEventAnimationDuration;
    [SerializeField] private float cursorFollowSlowFactor;

    private const Ease CURSOR_EXPO_EASE = Ease.OutExpo;

    private SFXManager sfxManager;
    private ChatManager chatManager;
    private TMP_Text inputText;
    List<string> inputTextInputs;
    private Transform inputIcons;
    private Image draggingImage;
    private Button sendButton;
    private Transform activeChoices;
    private Vector3 cursorOriginalScale;

    private bool isMultiAnswerTextInput;
    private bool isIconsInput;
    private string[] input;
    private bool cursorDragging;
    private int lastUsedIconIndex;


    private void Awake()
    {
        sfxManager = FindObjectOfType<SFXManager>();

        lastUsedIconIndex = -1;
        chatManager = FindObjectOfType<ChatManager>();
        inputText = transform.Find("PlayerInputText").GetComponent<TMP_Text>();
        inputText.text = "";
        input = null;

        inputIcons = transform.Find("PlayerInputIcons");
        (sendButton = transform.Find("SendButton").GetComponent<Button>()).onClick.AddListener(() => { TakeInput(); });
        sendButton.interactable = false;

        BindIcons();
        BindTextButtons();
        BindIconEventsForImage();
        BindIconEventsForAudio();

        EventTrigger.Entry iconDrop = new EventTrigger.Entry();
        iconDrop.eventID = EventTriggerType.Drop;
        iconDrop.callback.AddListener((eventData) => { OnIconDrop(); });

        cursorOriginalScale = cursorIcon.transform.localScale;
        transform.Find("InputCheckBox").GetComponent<EventTrigger>().triggers.Add(iconDrop);

        ResetIconBar();

        inputTextInputs = new List<string>();

    }

    private void Update()
    {
        if (cursorDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = cursorIcon.transform.position.z;
            cursorIcon.transform.DOMove(mousePos, cursorFollowSlowFactor * Time.deltaTime);
        }
    }

    private void ResetIconBar()
    {
        lastUsedIconIndex = -1;

        for (int i = 0; i < inputIcons.childCount; i++)
        {
            Image imageIcon = inputIcons.GetChild(i).GetComponent<Image>();
            imageIcon.sprite = null;
            imageIcon.enabled = false;
        }
    }

    private void OnIconDrop()
    {
        if (draggingImage == null) return;

        string name = draggingImage.transform.name;
        int index = draggingImage.transform.parent.GetSiblingIndex();
        IconOnEndDrag(true);

        AddIcon(cursorIcon.sprite, name, index);

    }

    private void AddIcon(Sprite icon, string name, int index)
    {
        if (lastUsedIconIndex + 1 == inputIcons.childCount) { cursorIcon.sprite = null; return; }

        Image addedIconImage = inputIcons.GetChild(++lastUsedIconIndex).GetComponent<Image>();
        addedIconImage.sprite = icon;

        addedIconImage.transform.name = "Icon@" + index;
        cursorIcon.transform.DOMove(addedIconImage.transform.position, cursorFastEventAnimationDuration).SetEase(CURSOR_EXPO_EASE)
        .OnComplete(() => { addedIconImage.enabled = true; cursorIcon.sprite = null; });

        if (!sendButton.interactable) sendButton.interactable = true;

    }

    private void DiscardIcon(int index, bool isDrag = false)
    {
        Image discardImage = inputIcons.GetChild(index).GetComponent<Image>();

        if (!discardImage.enabled) return;

        if (!isDrag)
        {
            sfxManager.PlaySFX("discard", false);
            clickCircle.SafeSetPosition(discardImage.transform.position);
            clickCircle.Fire();
            int choiceIndex = Int32.Parse(discardImage.transform.name.Split('@')[1]);


            Image choiceRoot = activeChoices.transform.GetChild(choiceIndex).GetChild(0).GetComponent<Image>();

            cursorIcon.transform.localScale = cursorOriginalScale * 0.25f;
            cursorIcon.transform.position = discardImage.transform.position;

            cursorIcon.gameObject.SetActive(true);
            cursorIcon.sprite = discardImage.sprite;

            cursorIcon.transform.DOScale(cursorOriginalScale * 0.5f, cursorFastEventAnimationDuration).SetEase(CURSOR_EXPO_EASE)
            .OnComplete(() =>
            { cursorIcon.transform.localScale = cursorOriginalScale; cursorIcon.gameObject.SetActive(false); cursorIcon.sprite = null; choiceRoot.enabled = true; });

            cursorIcon.transform.DOMove(choiceRoot.transform.position, cursorFastEventAnimationDuration);
        }


        Vector2 discardImageOriginalSize = discardImage.transform.localScale;

        discardImage.transform.DOScale(Vector3.zero, cursorFastEventAnimationDuration).SetEase(CURSOR_EXPO_EASE).OnComplete(() =>
        { discardImage.transform.localScale = discardImageOriginalSize; SlideIcons(index); });

        lastUsedIconIndex--;

        if (lastUsedIconIndex < 0) sendButton.interactable = false;
    }

    private void SlideIcons(int index)
    {

        for (int i = index; i + 1 <= inputIcons.childCount; i++)
        {
            Image currImage = inputIcons.GetChild(i).GetComponent<Image>();
            if (i + 1 == inputIcons.childCount)
            {
                currImage.enabled = false;
                currImage.transform.name = "Icon";
                currImage.sprite = null;
                return;
            }


            Image nextImage = inputIcons.GetChild(i + 1).GetComponent<Image>();

            if (!nextImage.enabled) { currImage.enabled = false; currImage.sprite = null; currImage.transform.name = "Icon"; break; }

            string nextImageChoiceIndex = nextImage.transform.name.Split('@')[1];
            currImage.sprite = nextImage.sprite;
            currImage.name = "Icon@" + nextImageChoiceIndex;
            Vector2 currImagePos = currImage.transform.position;
            LayoutElement currImageLayout = currImage.GetComponent<LayoutElement>();
            currImageLayout.ignoreLayout = true;
            currImage.transform.position = nextImage.transform.position;
            currImage.transform.DOMove(currImagePos, cursorFastEventAnimationDuration).SetEase(CURSOR_EXPO_EASE)
            .OnComplete(() => { currImageLayout.ignoreLayout = false; });

        }

    }

    private void IconOnEndDrag(bool success = false)
    {
        if (draggingImage == null) return;

        sfxManager.PlaySFX(success ? "drop" : "discard", false);

        if (!success)
        {
            int choiceIndex = Int32.Parse(cursorIcon.transform.name.Split('@')[1]);
            Image draggedImage = activeChoices.transform.GetChild(choiceIndex).GetChild(0).GetComponent<Image>();
            cursorIcon.transform.DOMove(draggedImage.transform.position, cursorFastEventAnimationDuration).OnComplete(() =>
             draggedImage.enabled = true);
        }

        draggingImage = null;
        cursorDragging = false;

        Vector3 originalScale = cursorIcon.transform.localScale;

        cursorIcon.transform.DOScale(!success ? originalScale * 0.5f : originalScale * 0.25f, cursorFastEventAnimationDuration).SetEase(CURSOR_EXPO_EASE)
        .OnComplete(() =>
        { cursorIcon.transform.localScale = originalScale; cursorIcon.gameObject.SetActive(false); });



    }

    private void ChoiceIconOnClick(Image clickedImage)
    {
        if (lastUsedIconIndex + 1 == inputIcons.childCount) return;
        if (!clickedImage.CompareTag("ChoiceImage") && !clickedImage.CompareTag("ChoiceAudio") && !clickedImage.CompareTag("PlayerInputIcon")) return;

        Vector3 imagePrevScale = clickedImage.transform.localScale;
        clickedImage.transform.DOScale(Vector3.zero, cursorFastEventAnimationDuration).SetEase(CURSOR_EXPO_EASE)
        .OnComplete(() => { clickedImage.enabled = false; clickedImage.transform.localScale = imagePrevScale; });

        sfxManager.PlaySFX("choice click", false);
        cursorIcon.transform.localScale = cursorOriginalScale * 0.5f;

        cursorIcon.transform.position = clickedImage.transform.position;
        clickCircle.SafeSetPosition(clickedImage.transform.position);
        clickCircle.Fire();

        cursorIcon.gameObject.SetActive(true);
        cursorIcon.sprite = clickedImage.sprite;

        cursorIcon.transform.DOScale(cursorOriginalScale * 0.25f, cursorFastEventAnimationDuration).SetEase(CURSOR_EXPO_EASE)
        .OnComplete(() =>
        { cursorIcon.transform.localScale = cursorOriginalScale; cursorIcon.gameObject.SetActive(false); cursorIcon.sprite = null; });

        AddIcon(clickedImage.sprite, clickedImage.transform.name, clickedImage.transform.parent.GetSiblingIndex());
    }

    private void IconOnBeginDrag(Image _draggingImage, bool isInputIcon = false)
    {
        if (!_draggingImage.enabled) return;
        if (!_draggingImage.CompareTag("ChoiceImage") && !_draggingImage.CompareTag("ChoiceAudio") && !_draggingImage.CompareTag("PlayerInputIcon")) return;
        if (!_draggingImage.CompareTag("PlayerInputIcon") && lastUsedIconIndex + 1 == inputIcons.childCount) return;

        sfxManager.PlaySFX("choice drag", false);

        _draggingImage.enabled = isInputIcon;
        draggingImage = _draggingImage;
        cursorDragging = true;


        cursorIcon.transform.name = "CursorIcon@" + (!isInputIcon ? _draggingImage.transform.parent.GetSiblingIndex()
        : inputIcons.GetChild(_draggingImage.transform.GetSiblingIndex()).name.Split('@')[1]);
        cursorIcon.transform.position = _draggingImage.transform.position;
        cursorIcon.transform.localScale = Vector3.zero;


        cursorIcon.transform.DOScale(cursorOriginalScale, cursorSmoothEventAnimationDuration).SetEase(CURSOR_EXPO_EASE);



        cursorIcon.gameObject.SetActive(true);
        cursorIcon.sprite = _draggingImage.sprite;

    }

    private void BindIconEventsForImage()
    {
        EventTrigger.Entry pointerEndDrag = new EventTrigger.Entry();
        pointerEndDrag.eventID = EventTriggerType.EndDrag;
        pointerEndDrag.callback.AddListener((eventData) => { IconOnEndDrag(); });


        for (int i = 0; i < imageChoices.childCount; i++)
        {


            RectTransform anIcon = imageChoices.GetChild(i).GetChild(0).GetComponent<RectTransform>();
            Image iconImage = anIcon.GetComponent<Image>();

            EventTrigger iconEvent = anIcon.GetComponent<EventTrigger>();

            EventTrigger.Entry pointerBeginDrag = new EventTrigger.Entry();
            pointerBeginDrag.eventID = EventTriggerType.BeginDrag;
            pointerBeginDrag.callback.AddListener((eventData) => { IconOnBeginDrag(iconImage); });

            EventTrigger.Entry pointerClick = new EventTrigger.Entry();
            pointerClick.eventID = EventTriggerType.PointerClick;
            pointerClick.callback.AddListener((eventData) => { ChoiceIconOnClick(iconImage); });


            iconEvent.triggers.Add(pointerBeginDrag);
            iconEvent.triggers.Add(pointerEndDrag);
            iconEvent.triggers.Add(pointerClick);

        }
    }
    private void BindIconEventsForAudio()
    {
        EventTrigger.Entry pointerEndDrag = new EventTrigger.Entry();
        pointerEndDrag.eventID = EventTriggerType.EndDrag;
        pointerEndDrag.callback.AddListener((eventData) => { IconOnEndDrag(); });

        for (int i = 0; i < audioChoices.childCount; i++)
        {
            RectTransform anIcon = audioChoices.GetChild(i).GetChild(0).GetComponent<RectTransform>();
            Image iconImage = anIcon.GetComponent<Image>();

            EventTrigger iconEvent = anIcon.GetComponent<EventTrigger>();

            EventTrigger.Entry pointerBeginDrag = new EventTrigger.Entry();
            pointerBeginDrag.eventID = EventTriggerType.BeginDrag;
            pointerBeginDrag.callback.AddListener((eventData) => { IconOnBeginDrag(iconImage); });

            iconEvent.triggers.Add(pointerBeginDrag);
            iconEvent.triggers.Add(pointerEndDrag);

        }
    }
    private void TakeInput()
    {
        if (!chatManager.IsAcceptInput())
            return;

        if (isIconsInput)
        {
            input = new string[] { GetIconText() };
            ResetIconBar();
        }

        else
        {
            input = inputTextInputs.ToArray();
            ClearInputText();
        }

        sendButton.interactable = false;
    }
    private void BindTextButtons()
    {
        for (int i = 0; i < textChoices.childCount; i++)
        {
            Transform aChoice = textChoices.GetChild(i);
            Button button = aChoice.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                Image image = aChoice.GetComponent<Image>();
                if (image.color == Color.white)
                {
                    if (SetInputText(aChoice))
                        image.color = button.colors.selectedColor;
                }

                else if (image.color == button.colors.selectedColor)
                {
                    if (ClearInputTextPartial(aChoice))
                        image.color = Color.white;
                }

            });

        }
    }
    private void BindIcons()
    {
        EventTrigger.Entry pointerEndDrag = new EventTrigger.Entry();
        pointerEndDrag.eventID = EventTriggerType.EndDrag;
        pointerEndDrag.callback.AddListener((eventData) => { IconOnEndDrag(false); });

        for (int i = 0; i < inputIcons.childCount; i++)
        {
            Transform anIcon = inputIcons.GetChild(i);
            EventTrigger iconTrigger = anIcon.GetComponent<EventTrigger>();
            Image iconImage = anIcon.GetComponent<Image>();

            int iconIndex = i;

            EventTrigger.Entry pointerBeginDrag = new EventTrigger.Entry();
            pointerBeginDrag.eventID = EventTriggerType.BeginDrag;
            pointerBeginDrag.callback.AddListener((eventData) => { IconOnBeginDrag(iconImage, true); DiscardIcon(iconIndex, true); });

            EventTrigger.Entry pointerClick = new EventTrigger.Entry();
            pointerClick.eventID = EventTriggerType.PointerClick;
            pointerClick.callback.AddListener((eventData) => { DiscardIcon(iconIndex); });

            iconTrigger.triggers.Add(pointerBeginDrag);
            iconTrigger.triggers.Add(pointerClick);
            iconTrigger.triggers.Add(pointerEndDrag);
        }

    }
    private bool SetInputText(Transform setter)
    {
        string addedText = setter.Find("Text (TMP)").GetComponent<TMP_Text>().text;


        if (!isMultiAnswerTextInput && inputTextInputs.Count != 0)
        {
            ClearInputText();
        }

        if (!inputTextInputs.Contains(addedText))
        {
            inputTextInputs.Add(addedText);
            UpdateInputText();
            return true;
        }

        return false;
    }

    private void UpdateInputText()
    {
        string updated = "";
        for (int i = 0; i < inputTextInputs.Count; i++)
        {
            if (i != 0)
            {
                updated += ", ";
            }

            updated += inputTextInputs[i];
        }

        inputText.text = updated;
        sendButton.interactable = inputTextInputs.Count > 0;
    }
    private void SetInputText(string text)
    {
        inputTextInputs.Add(text);
        UpdateInputText();
    }
    private void ClearInputText()
    {
        inputTextInputs.Clear();
        UpdateInputText();
        for (int i = 0; i < textChoices.childCount; i++)
        {
            textChoices.GetChild(i).GetComponent<Image>().color = Color.white;
        }
    }

    private bool ClearInputTextPartial(Transform setter)
    {
        string deletedText = setter.Find("Text (TMP)").GetComponent<TMP_Text>().text;
        if (inputText.text.Contains(deletedText))
        {
            inputTextInputs.Remove(deletedText);
            UpdateInputText();
            return true;
        }

        return false;
    }

    private string GetIconText()
    {
        string iconText = "";
        Image iconImage;
        for (int i = 0; i < inputIcons.childCount && (iconImage = inputIcons.GetChild(i).GetComponent<Image>()).enabled; i++)
        {
            if (i != 0)
            {
                iconText += ";";
            }

            iconText += activeChoices.GetChild(Int32.Parse(iconImage.transform.name.Split('@')[1])).GetChild(0).name;
        }

        return iconText;
    }

    public void SetStartInputText()
    {
        SetInputText("Je suis prêt/e.");
        //SetInputText("I am ready!");
        //SetInputText("Hazırım!");
    }
    public void MakeIconInput(bool val = true)
    {
        isIconsInput = val;
    }
    public void MakeMultiTextInput(bool val = true)
    {
        isMultiAnswerTextInput = val;
    }
    public string[] GetInput()
    {
        return input;
    }
    public void ClearInput()
    {
        input = null;

    }
    public void DisableAllChoices()
    {
        textChoices.gameObject.SetActive(false);
        imageChoices.gameObject.SetActive(false);
        audioChoices.gameObject.SetActive(false);
    }
    public void SetChoices(string[] choices, string type)
    {
        Transform choicesHolder = null;
        if (type == "text")
        {
            choicesHolder = textChoices.transform;
            for (int i = 0; i < textChoices.childCount; i++)
            {
                textChoices.GetChild(i).Find("Text (TMP)").GetComponent<TMP_Text>().text = choices[i];
                textChoices.GetChild(i).GetComponent<Image>().enabled = choices[i].Length > 0;
            }

        }

        else if (type == "image")
        {
            choicesHolder = imageChoices;

            for (int i = 0; i < imageChoices.childCount; i++)
            {
                Image choiceImage = imageChoices.GetChild(i).GetChild(0).GetComponent<Image>();
                choiceImage.transform.name = choices[i];
                choiceImage.sprite = Resources.Load<Sprite>("Images/" + choices[i]);

                if (!choiceImage.enabled)
                {
                    Vector3 imagePrevScale = choiceImage.transform.localScale;
                    choiceImage.transform.localScale = Vector3.zero;
                    choiceImage.enabled = true;

                    choiceImage.transform.DOScale(imagePrevScale, cursorFastEventAnimationDuration).SetEase(CURSOR_EXPO_EASE);
                }


            }

        }

        else if (type == "audio")
        {
            choicesHolder = audioChoices;

            for (int i = 0; i < audioChoices.childCount; i++)
            {
                string audioName = choices[i];
                Image buttonImage = audioChoices.GetChild(i).GetChild(0).GetComponent<Image>();

                if (!buttonImage.enabled)
                {
                    Vector3 imagePrevScale = buttonImage.transform.localScale;
                    buttonImage.transform.localScale = Vector3.zero;
                    buttonImage.enabled = true;

                    buttonImage.transform.DOScale(imagePrevScale, cursorFastEventAnimationDuration).SetEase(CURSOR_EXPO_EASE);
                }


                buttonImage.transform.name = audioName;
                buttonImage.GetComponent<Button>().onClick.AddListener(() =>
                { audioSpeaker.PlayOneShot(Resources.Load<AudioClip>("Audio/" + audioName)); /*soundPlaying.SafeSetPosition(buttonImage); soundPlaying.Fire();*/});
            }
        }

        activeChoices = choicesHolder;
        activeChoices.gameObject.SetActive(true);
    }
    public Transform GetActiveChoices()
    {
        return activeChoices;
    }

}

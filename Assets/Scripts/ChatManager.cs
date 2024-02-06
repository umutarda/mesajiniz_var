using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System;

public class ChatManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject playerTextMessagePrefab;
    [SerializeField] private GameObject playerImageMessagePrefab;
    [SerializeField] private GameObject playerAudioMessagePrefab;
    [SerializeField] private GameObject questionerTextMessagePrefab;
    [SerializeField] private GameObject questionerAudioMessagePrefab;
    [SerializeField] private GameObject questionerImageMessagePrefab;
    [SerializeField] private GameObject questionerMessageWaitPrefab;


    [Header("Scene Objects")]
    [SerializeField] private Transform messagesHolder;
    [SerializeField] private RectTransform phoneLayoutRectTransform;
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private AudioSource audioSpeaker;
    [SerializeField] private UIParticle confettiUIParticle;
    [SerializeField] private UIParticle incorrectRainUIParticle;
    [SerializeField] private UIParticle sendCorrectUIParticle;


    [Header("Design Parameters")]
    [SerializeField] private float choicesExpansionAnimationDuration;
    [SerializeField] private float choicesRevealAnimationDuration;
    [SerializeField] private float questionerMessagesSlideAnimationDuration;
    [SerializeField] private float playerMessagesSlideAnimationDuration;
    [SerializeField] private float messageWaitRevealRatioToSlide;
    [SerializeField] private float messageRevealAnimationDuration;
    [SerializeField] private float messageWaitDotRevealAnimationDuration;
    [SerializeField] private float messageWaitRevealAnimationNoDotDuration;
    [SerializeField] private float waitAfterFinalMessageDuration;
    [SerializeField] private float messageWaitDurationAfterReveal;
    [SerializeField] private int choicesMessageVerticalSize;
    [SerializeField] private float messageVerticalExcess;
    [SerializeField] private float messageVerticalPadding;
    [SerializeField] private float questionerWriteDelay;
    [SerializeField] private float messageExpansionSmoothFactor;

    [SerializeField] private string questionsFileName;
    public string QuestionsFileName => questionsFileName;

    private const Ease CHOICES_EXPANSION_EASE = Ease.InElastic;
    private const Ease CHOICES_SHRINK_EASE = Ease.OutElastic;
    private const Ease CHOICES_REVEAL_EASE = Ease.InOutCubic;
    private const Ease MESSAGES_SLIDE_EASE = Ease.OutCubic;
    private const Ease MESSAGE_REVEAL_EASE = Ease.OutQuint;
    private const Ease MESSAGE_WAIT_DOT_REVEAL_EASE = Ease.OutExpo;

    private SmileController smileController;
    private SFXManager sfxManager;
    private Vector2 layoutSizeWithoutChoices, layoutSizeWithChoices;
    private RectTransform oldMessagesHolder;
    private Transform lastMessage;
    private Transform questionerWaitT;
    private IEnumerator currentWriteNumerator;
    private IEnumerator choicesAnimationNumerator;
    private WaitForSecondsRealtime questionerWriteWFS, choicesExpansionWFS, choicesRevealWFS,
    playerMessageSlideWFS, messageRevealWFS, messageWaitAfterRevealWFS, waitAfterFinalMessageWFS;
    private Sequence messageWaitDotSequence;
    private bool choicesAreShown;
    private bool playerAnswered;
    private float lastTimePlayerInputted;
    private int correctAnswerSequence;
    private bool isStart;
    private bool isEnd;
    private const int MESSAGE_BOX_LINE_CHAR_COUNT = 30;

    private void Awake()
    {
        layoutSizeWithoutChoices = phoneLayoutRectTransform.sizeDelta;
        layoutSizeWithChoices = layoutSizeWithoutChoices + Vector2.up * choicesMessageVerticalSize;

        questionerWriteWFS = new WaitForSecondsRealtime(questionerWriteDelay);
        choicesExpansionWFS = new WaitForSecondsRealtime(choicesExpansionAnimationDuration);
        choicesRevealWFS = new WaitForSecondsRealtime(choicesRevealAnimationDuration);
        playerMessageSlideWFS = new WaitForSecondsRealtime(playerMessagesSlideAnimationDuration);
        messageWaitAfterRevealWFS = new WaitForSecondsRealtime(messageWaitDurationAfterReveal);
        waitAfterFinalMessageWFS = new WaitForSecondsRealtime(waitAfterFinalMessageDuration);


        oldMessagesHolder = messagesHolder.GetChild(0).GetComponent<RectTransform>();
        questionerWaitT = Instantiate(questionerMessageWaitPrefab, messagesHolder).GetComponent<RectTransform>();
        questionerWaitT.gameObject.SetActive(false);

        smileController = FindObjectOfType<SmileController>();
        sfxManager = FindObjectOfType<SFXManager>();

        messageWaitDotSequence = DOTween.Sequence();
        for (int i = 0; i < questionerWaitT.childCount; i++)
        {
            Transform aDot = questionerWaitT.GetChild(i);
            messageWaitDotSequence.Append(aDot.DOScale(aDot.localScale, messageWaitDotRevealAnimationDuration).SetEase(MESSAGE_WAIT_DOT_REVEAL_EASE));
            messageWaitDotSequence.SetDelay(messageWaitRevealAnimationNoDotDuration);
        }

        messageWaitDotSequence.SetLoops(-1);
        ResetMessageWaitDotScales();

        Questioner.ResetQuestioner();

    }

    private void Start()
    {
        string[] start = /*new string[] {"Merhaba Mesajınız Var'a hoşgeldin!",
        "Ben sana sorular soracağım sen de alttan açılan seçenek menüsünden doğru cevabı göndermeye çalışacaksın.",//"Ekranın sağında benim okuluma ait bir kroki göreceksin. Bu krokiyle ilgili sana sorular soracağım. Sen de alttan açılan seçenek menüsünden doğru cevabı göndermeye çalışacaksın.",
        "Hazır mısın?"};*/
        /*new string[] {"Welcome to Mesajınız Var!",
        "I will ask you questions and you will try to send the correct answer from the option menu that opens at the bottom.",//"Ekranın sağında benim okuluma ait bir kroki göreceksin. Bu krokiyle ilgili sana sorular soracağım. Sen de alttan açılan seçenek menüsünden doğru cevabı göndermeye çalışacaksın.",
        "Are you ready?"};*/
        new string[] {"Bonjour et bienvenue à “Vous avez reçu un message!”",
        "Je vais vous poser des questions et vous allez essayer de choisir la bonne réponse parmi les choix.",//"Ekranın sağında benim okuluma ait bir kroki göreceksin. Bu krokiyle ilgili sana sorular soracağım. Sen de alttan açılan seçenek menüsünden doğru cevabı göndermeye çalışacaksın.",
        "Vous êtes prêt/e ?"};

        smileController.PlayAnimation(UnityEngine.Random.value >= .5f ? "idle1" : "idle2");
        smileController.LoopAnimation();

        currentWriteNumerator = QuestionerWriteSequence(start, "prompt", true);
        StartCoroutine(currentWriteNumerator);

        isStart = true;
    }
    private void ResetMessageWaitDotScales()
    {
        for (int i = 0; i < questionerWaitT.childCount; i++)
        {
            questionerWaitT.GetChild(i).localScale = Vector3.zero;
        }
    }
    private void Update()
    {
        if (currentWriteNumerator != null)
            return;

        if (isStart)
        {
            string[] input = playerInputManager.GetInput();
            if (input?.Length > 0)
            {
                if (!playerAnswered)
                {
                    smileController.KillAnimation();
                    Vector2 oldMessageNewPos;
                    string inputText = "";
                    for (int i = 0; i < input.Length; i++)
                    {
                        if (i != 0)
                            inputText += ", ";
                        inputText += input[i];
                    }
                    currentWriteNumerator = SmoothWrite(Write(true, inputText, out oldMessageNewPos, "text"), oldMessageNewPos);
                    StartCoroutine(currentWriteNumerator);
                    playerAnswered = true;
                    sendCorrectUIParticle.Fire();
                    sfxManager.PlaySFX("send correct");

                }

                else
                {
                    playerInputManager.ClearInput();
                    playerAnswered = false;
                    isStart = false;
                }

            }
        }

        else if (isEnd)
        {

            currentWriteNumerator = Closing();
            StartCoroutine(currentWriteNumerator);
        }

        else
        {
            Chat();
        }

    }
    private void Chat()
    {

        string[] questionerResponse = null;
        string responseType = "";
        if (Questioner.IsThereCurrentQU())
        {
            if (!choicesAreShown)
            {
                if (choicesAnimationNumerator == null)
                {
                    playerInputManager.DisableAllChoices();
                    choicesAnimationNumerator = ChoicesAnimation(layoutSizeWithChoices);
                    StartCoroutine(choicesAnimationNumerator);
                    smileController.KillAnimation();
                }

            }

            else
            {
                string[] input = playerInputManager.GetInput();
                if (input?.Length > 0)
                {
                    QuestionUnitType qut = ReadXML.GetQuestionUnitType(Questioner.GetCurrentQU().GetTypeString());
                    string choiceType = qut.GetChoiceTypeString();

                    if (!playerAnswered)
                    {
                        Vector2 oldMessageNewPos;
                        string inputText = "";
                        for (int i = 0; i < input.Length; i++)
                        {
                            if (i != 0)
                                inputText += ", ";
                            inputText += input[i];
                        }
                        currentWriteNumerator = SmoothWrite(Write(true, inputText, out oldMessageNewPos, choiceType), oldMessageNewPos);
                        StartCoroutine(currentWriteNumerator);
                        playerAnswered = true;

                        if (qut.GetTypeString().Equals("fun_questions_request"))
                        {
                            sendCorrectUIParticle.Fire();
                            sfxManager.PlaySFX("send correct");
                        }

                        else
                        {
                            Debug.Log ("IS CORRECT = " + Questioner.IsCorrect(playerInputManager.GetInput()));
                            if (Questioner.IsCorrect(playerInputManager.GetInput()))
                            {
                                sendCorrectUIParticle.Fire();
                                sfxManager.PlaySFX("send correct");
                            }

                            else
                            {
                                sfxManager.PlaySFX("message sent");
                            }
                        }

                    }

                    else
                    {
                        bool correctAnswer;
                        questionerResponse = Questioner.GetResponse(playerInputManager.GetInput(), out correctAnswer);
                        playerInputManager.ClearInput();


                        if (correctAnswer)
                        {
                            responseType = "correct";
                            correctAnswerSequence++;
                        }

                        else
                        {
                            responseType = "incorrect";
                            correctAnswerSequence = 0;

                        }

                        if (!correctAnswer)
                        {

                            if (Questioner.GetCurrentQU() != null)
                                playerInputManager.SetChoices(Questioner.GetCurrentQU().GetChoicesStrings(), choiceType);


                        }

                        if (qut.GetTypeString().Equals("fun_questions_request"))
                            responseType = "";

                        playerAnswered = false;
                    }

                }

                else
                {
                    //Debug.Log("is trip: " + (Time.timeSinceLevelLoad - lastTimePlayerInputted >= smileController.GetTripThreshold()));
                    if (Time.timeSinceLevelLoad - lastTimePlayerInputted >= smileController.GetTripThreshold())
                    {
                        if (smileController.GetAnimationPlayedName() != "trip")
                        {
                            smileController.PlayAnimation("trip");
                            sfxManager.PlaySFX("trip", false);
                        }
                    }
                }
            }

        }

        else
        {

            if (choicesAreShown)
            {
                if (choicesAnimationNumerator == null)
                {
                    choicesAnimationNumerator = ChoicesAnimation(layoutSizeWithoutChoices);
                    StartCoroutine(choicesAnimationNumerator);
                }
            }

            else
            {
                bool beenPut;
                responseType = "prompt";
                questionerResponse = Questioner.PutQU(out beenPut);

                Debug.Log(beenPut);
                isEnd = !beenPut;
                if (!smileController.GetAnimationPlayedName().Contains("idle")) smileController.PlayAnimation(UnityEngine.Random.value >= .5f ? "idle1" : "idle2");

            }


        }

        if (questionerResponse != null && !isEnd)
        {
            currentWriteNumerator = QuestionerWriteSequence(questionerResponse, responseType);
            StartCoroutine(currentWriteNumerator);
        }


    }
    private GameObject Write(bool isPlayer, string message, out Vector2 oldMessagesNewPosition, string type = "text")
    {
        GameObject prefab = null;

        //DETERMINE PREFAB TYPE
        if (isPlayer)
        {
            if (type == "text")
            {
                prefab = playerTextMessagePrefab;
            }

            else if (type == "audio")
            {
                prefab = playerAudioMessagePrefab;
            }

            else if (type == "image")
            {
                prefab = playerImageMessagePrefab;
            }
        }

        else
        {
            if (type == "text")
            {
                prefab = questionerTextMessagePrefab;
            }

            else if (type == "audio")
            {
                prefab = questionerAudioMessagePrefab;
            }

            else if (type == "image")
            {
                prefab = questionerImageMessagePrefab;
            }
        }

        GameObject newMessage = Instantiate(prefab, messagesHolder);
        float scaleFactor = 1;

        //PUT CONTENT INTO THE MESSAGE
        if (type == "text")
        {
            newMessage.transform.Find("MessageText").GetComponent<TMP_Text>().text = message;
            RectTransform newMessageRectT = newMessage.GetComponent<RectTransform>();
            int lineCount = Mathf.CeilToInt(1.0f * (message.Length) / MESSAGE_BOX_LINE_CHAR_COUNT);
            scaleFactor = .5f * lineCount;
            if (lineCount > 2)
            {
                newMessageRectT.sizeDelta = new Vector2(newMessageRectT.sizeDelta.x, scaleFactor * newMessageRectT.sizeDelta.y);

                Vector2 startPos = newMessage.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
                float messageHeight = newMessage.transform.GetChild(0).GetComponent<RectTransform>().rect.height;
                float lastPos = 0;

                RectTransform topRect = newMessage.transform.GetChild(1).GetComponent<RectTransform>();

                int i = 1;
                while (lastPos < -topRect.rect.height + newMessageRectT.rect.height / 2)
                {
                    lastPos = (messageHeight - messageExpansionSmoothFactor) * i++;

                    Transform up = Instantiate(newMessage.transform.GetChild(0), newMessage.transform);
                    up.GetComponent<RectTransform>().anchoredPosition = startPos + Vector2.up * (lastPos);
                    up.SetAsFirstSibling();


                    Transform down = Instantiate(newMessage.transform.GetChild(0), newMessage.transform);
                    down.GetComponent<RectTransform>().anchoredPosition = startPos + Vector2.down * (lastPos);
                    down.SetAsFirstSibling();

                }
            }

        }

        else if (type == "audio")
        {
            Transform audioHolder = newMessage.transform.Find("MessageAudio");
            int maxAudioCount = audioHolder.childCount;
            string[] audioNames = message.Split(";");

            for (int i = 0; i < audioNames.Length && i < maxAudioCount; i++)
            {
                string audioName = audioNames[i];
                Transform anAudio = audioHolder.GetChild(i);
                anAudio.GetComponent<Button>().onClick.AddListener(() =>
                { audioSpeaker.PlayOneShot(Resources.Load<AudioClip>("Audio/" + audioName)); /*soundPlaying.SafeSetPosition(anAudio); soundPlaying.Fire();*/});

                anAudio.gameObject.SetActive(true);
            }
        }

        else if (type == "image")
        {
            Transform imagesHolder = newMessage.transform.Find("MessageImages");
            int maxImageCount = imagesHolder.childCount;
            string[] imageNames = message.Split(";");

            for (int i = 0; i < imageNames.Length && i < maxImageCount; i++)
            {
                imagesHolder.GetChild(i).GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/" + imageNames[i]);
                imagesHolder.GetChild(i).gameObject.SetActive(true);
            }

        }

        Vector2 oldMessagesNewPos = oldMessagesHolder.anchoredPosition;
        if (lastMessage != null)
        {
            lastMessage.SetParent(oldMessagesHolder);

            float messageHeightHalf = newMessage.GetComponent<RectTransform>().rect.height / 2;
            oldMessagesNewPos += Vector2.up * (messageHeightHalf + scaleFactor * messageVerticalExcess + messageVerticalPadding);
        }

        lastMessage = newMessage.transform;

        oldMessagesNewPosition = oldMessagesNewPos;
        return newMessage;

    }
    private IEnumerator QuestionerWriteSequence(string[] messages, string responseType, bool isStart = false)
    {
        IEnumerator subroutine = null;
        Debug.Log(responseType);
        for (int i = 0; i < messages.Length; i++)
        {
            sfxManager.PlaySFX("typing");
            string[] typeSplit = messages[i].Split("@");
            string mType = typeSplit.Length == 2 ? typeSplit[1] : "normal_text";
            string message = mType != "normal_text" ? typeSplit[0] : messages[i];

            if (mType == "normal_text") mType = "text";

            Vector2 oldMessagePos;
            subroutine = SmoothWrite(Write(false, message, out oldMessagePos, mType), oldMessagePos, true);
            StartCoroutine(subroutine);
            bool isDone = false;
            while (!isDone)
            {
                yield return false;

                if (subroutine.Current is bool)
                    isDone = (bool)(subroutine.Current);

                else if (subroutine.Current is string) //"revealed" string
                {
                    if (i + 1 == messages.Length && responseType == "correct")
                    {
                        smileController.PlayAnimation(correctAnswerSequence >= smileController.GetCoolWinCorrectAnswerThreshold() ? "win2" : "win1");
                        confettiUIParticle.Fire();
                        sfxManager.PlaySFX
                        (correctAnswerSequence >= smileController.GetCoolWinCorrectAnswerThreshold() ? "correct sequence" : "correct answer", false);
                    }

                    if (i == 0 && responseType == "incorrect")
                    {
                        smileController.PlayAnimation(UnityEngine.Random.value >= .5f ? "lose1" : "lose2");
                        incorrectRainUIParticle.Fire();
                        sfxManager.PlaySFX("wrong answer", false);
                    }
                }
            }

        }

        if (isStart) { playerInputManager.SetStartInputText(); }

        lastTimePlayerInputted = Time.timeSinceLevelLoad;
        currentWriteNumerator = null;
    }
    private IEnumerator SmoothWrite(GameObject message, Vector2 oldMessagesPos, bool isQuestioner = false, bool waitAfterReveal = true)
    {
        message.SetActive(false);
        Vector3 messageTargetSize = message.transform.localScale;
        message.transform.localScale = Vector3.zero;

        if (isQuestioner)
        {
            bool isSliding = true;
            Tween slideTween = oldMessagesHolder.DOAnchorPos(oldMessagesPos, questionerMessagesSlideAnimationDuration)
            .SetEase(MESSAGES_SLIDE_EASE).OnComplete(() => isSliding = false);


            while (isSliding)
            {
                if (!questionerWaitT.gameObject.activeInHierarchy)
                {
                    if (slideTween.ElapsedPercentage() >= messageWaitRevealRatioToSlide)
                    {
                        ResetMessageWaitDotScales();
                        messageWaitDotSequence.Restart();

                        Vector2 waitTargetSize = questionerWaitT.transform.localScale;
                        questionerWaitT.transform.localScale = Vector3.zero;
                        questionerWaitT.gameObject.SetActive(true);
                        questionerWaitT.transform.DOScale(waitTargetSize, messageRevealAnimationDuration).SetEase(MESSAGE_REVEAL_EASE);
                    }

                }

                yield return null;
            }

            questionerWaitT.gameObject.SetActive(false);
        }

        else
        {
            oldMessagesHolder.DOAnchorPos(oldMessagesPos, playerMessagesSlideAnimationDuration).SetEase(MESSAGES_SLIDE_EASE);
            yield return playerMessageSlideWFS;
        }

        message.SetActive(true);
        message.transform.DOScale(messageTargetSize, messageRevealAnimationDuration).SetEase(MESSAGE_REVEAL_EASE);
        yield return messageRevealWFS;
        yield return "revealed";
        if (isQuestioner) sfxManager.PlaySFX("new message");
        yield return messageWaitAfterRevealWFS;

        if (isQuestioner)
        {
            yield return true;
        }

        else
        {
            currentWriteNumerator = null;
        }

    }

    private IEnumerator Closing()
    {
        bool isDone = false;
        Vector2 oldMessagePos;
        IEnumerator finalMessageRoutine = SmoothWrite(Write(false, "Bravo, vous avez bien répondu à toutes les questions !", out oldMessagePos), oldMessagePos, true); //SmoothWrite(Write(false, "Tebrikler tüm sorularımı cevapladın!", out oldMessagePos), oldMessagePos, true);
        StartCoroutine(finalMessageRoutine);
        while (!isDone)
        {
            yield return null;
            if (finalMessageRoutine.Current is bool)
                isDone = (bool)(finalMessageRoutine.Current);
        }

        Debug.Log("close");
        yield return waitAfterFinalMessageWFS;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private IEnumerator ChoicesAnimation(Vector2 targetSize)
    {

        sfxManager.PlaySFX("slide");
        bool isExpansion = targetSize == layoutSizeWithChoices;
        Transform choicesHolder;

        if (isExpansion)
        {
            phoneLayoutRectTransform.DOSizeDelta(targetSize, choicesExpansionAnimationDuration).SetEase(CHOICES_EXPANSION_EASE);
            yield return choicesExpansionWFS;

            QuestionUnitType qut = ReadXML.GetQuestionUnitType(Questioner.GetCurrentQU().GetTypeString());
            string choiceType = qut.GetChoiceTypeString();

            playerInputManager.SetChoices(Questioner.GetCurrentQU().GetChoicesStrings(), choiceType);
            choicesHolder = playerInputManager.GetActiveChoices();

            Vector2 choicesTargetSize = choicesHolder.localScale;
            choicesHolder.localScale = Vector3.zero;

            choicesHolder.DOScale(choicesTargetSize, choicesRevealAnimationDuration).SetEase(CHOICES_REVEAL_EASE);
            yield return choicesRevealWFS;

            choicesAreShown = true;
            playerInputManager.MakeIconInput(choiceType != "text");
            playerInputManager.MakeMultiTextInput(qut.GetTypeString() == "kroki_question");

            lastTimePlayerInputted = Time.timeSinceLevelLoad;
        }

        else
        {

            choicesHolder = playerInputManager.GetActiveChoices();
            Vector2 choicesInitialSize = choicesHolder.localScale;
            choicesHolder.DOScale(Vector3.zero, choicesRevealAnimationDuration).SetEase(CHOICES_REVEAL_EASE);
            yield return choicesRevealWFS;

            phoneLayoutRectTransform.DOSizeDelta(targetSize, choicesExpansionAnimationDuration).SetEase(CHOICES_SHRINK_EASE);
            yield return choicesExpansionWFS;

            choicesAreShown = false;
            playerInputManager.DisableAllChoices();
            choicesHolder.localScale = choicesInitialSize;
            if (!smileController.GetAnimationPlayedName().Contains("idle")) smileController.PlayAnimation(UnityEngine.Random.value >= .5f ? "idle1" : "idle2");
        }


        choicesAnimationNumerator = null;
    }
    public bool IsAcceptInput()
    {
        return (isStart || Questioner.IsThereCurrentQU() && choicesAreShown) && currentWriteNumerator == null;
    }

}

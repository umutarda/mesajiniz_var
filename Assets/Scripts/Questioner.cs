using System.Linq;
using UnityEngine;

public static class Questioner
{
    static ChatManager chatManager;
    private static Question[] questions;
    private static QuestionUnit currentQU;
    private static int currentQuestionIndex;

    static Questioner()
    {
        chatManager = GameObject.FindObjectOfType<ChatManager>();
        questions = ReadXML.ReadQuestions(chatManager.QuestionsFileName);
    }

    private static Question GetNextQuestion()
    {
        return (++currentQuestionIndex < questions.Length) ? questions[currentQuestionIndex] : null;
    }
    public static QuestionUnit GetCurrentQU()
    {
        return currentQU;
    }
    public static bool IsThereCurrentQU()
    {
        return currentQU != null;
    }

    public static void ResetQuestioner()
    {
        currentQuestionIndex = 0;

        for (int i = 0; i < questions.Length; i++) //starting i may not be 0 for all cases 
        {
            int randPlace = UnityEngine.Random.Range(i, questions.Length);

            Question temp = questions[i];
            questions[i] = questions[randPlace];
            questions[randPlace] = temp;

            questions[i].ResetCurrentQUIndex();
        }
    }

    public static string[] PutQU(out bool beenPut)
    {
        //IF THERE IS NO QU, PUT ONE 
        if (currentQU == null)
        {

            //PUT QU FROM THE CURRENT QUESTION
            currentQU = questions[currentQuestionIndex].GetNextQU();

            //IF THERE IS NO NEXT QU FROM THE CURRENT QUESTION
            if (currentQU == null)
            {
                //IF THERE IS A NEXT QUESTION
                if (GetNextQuestion() != null)
                {
                    //PUT QU FROM THE NEXT QUESTION
                    currentQU = questions[currentQuestionIndex].GetNextQU();
                }

                else
                {
                    //RETURN PROMPT OF THE END OF THE GAME
                    beenPut = false;
                    return new string[] { "game end" };
                }

            }

            QuestionUnitType currentQUType = ReadXML.GetQuestionUnitType(currentQU.GetTypeString());

            beenPut = true;

            //RETURN PROMPT OF THE QU HAS BEEN PUT
            //currentQUType.GetGeneralPromptString(), currentQU.GetPromptString()+"@"+currentQUType.GetPromptTypeString()
            if (currentQU.GetTypeString().Equals("ImageEng")) 
            {
                return new string[] { currentQUType.GetGeneralPromptString(), currentQU.GetPromptString() + "@" + currentQUType.GetPromptTypeString() };
            }

            return new string[] { currentQU.GetPromptString() + "@" + currentQUType.GetPromptTypeString() };

        }

        beenPut = false;
        return null;
    }
    public static string[] GetResponse(string[] playerMessage, out bool correctness)
    {
        if (currentQU != null)
        {
            QuestionUnitType currentQUType = ReadXML.GetQuestionUnitType(currentQU.GetTypeString());

            //Debug.Log("player message " + playerMessage);
            //Debug.Log("answer " + currentQU.GetAnswerString());
            //IF ANSWER IS CORRECT
            if (IsCorrect(playerMessage))
            {
                //NULLIFY CURRENT QU
                currentQU = null;

                correctness = true;
                //RETURN PROMPT OF A CORRECT FEEDBACK
                return new string[] { currentQUType.GetACorrectFeedback() };

            }

            else
            {
                correctness = false;

                if (currentQU.GetTypeString().Equals("fun_questions_request"))
                {
                    currentQU = null;
                    GetNextQuestion();
                    return new string[] { currentQUType.GetAnIncorrectFeedback() };

                }

                else
                {
                    //RETURN PROMPT OF A FALSE FEEDBACK AND REMINDER OF THE QU PROMPT
                    return new string[]{
                    currentQUType.GetAnIncorrectFeedback(),
                    currentQUType.GetReminderFeedbackString(),
                    //currentQUType.GetGeneralPromptString(),
                    currentQU.GetPromptString()+"@"+currentQUType.GetPromptTypeString()};
                }


            }


        }

        correctness = false;
        return null;
    }

    public static bool IsCorrect(string[] playerMessage)
    {
        

        foreach (var ans in currentQU.GetAnswers())
        {
            if (!playerMessage.Contains(ans)) 
            {
                Debug.Log("player message does not contain : " + ans);
                return false;
            }
                
        }
        return true;
    }

}

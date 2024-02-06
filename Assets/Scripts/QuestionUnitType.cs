using UnityEngine;
public class QuestionUnitType 
{
    private string typeName;
    private string promptType;
    private string choiceType;
    private string generalPrompt;
    private string reminderFeedback;
    private string[] correctFeedbacks;
    private string[] incorrectFeedbacks;

    public QuestionUnitType(string _typeName, string _promptType, string _choiceType, string _generalPrompt, string _reminderFeedback,
    string[] _correctFeedbacks, string[] _incorrectFeedbacks) 
    {
        typeName = _typeName;
        promptType = _promptType;
        choiceType = _choiceType;
        generalPrompt = _generalPrompt;
        reminderFeedback = _reminderFeedback;
        correctFeedbacks = _correctFeedbacks;
        incorrectFeedbacks = _incorrectFeedbacks;
    }

    public string GetTypeString() 
    {
        return typeName;
    } 

    public string GetPromptTypeString() 
    {
        return promptType;
    }

    public string GetChoiceTypeString() 
    {
        return choiceType;
    }

    public string GetGeneralPromptString() 
    {
        return generalPrompt;
    } 

    public string GetReminderFeedbackString() 
    {
        return reminderFeedback;
    } 

    public string GetACorrectFeedback() 
    {
        int rand = Random.Range(0,correctFeedbacks.Length);
        return correctFeedbacks[rand];
    } 

    public string GetAnIncorrectFeedback() 
    {
        int rand = Random.Range(0,incorrectFeedbacks.Length);
        return incorrectFeedbacks[rand];
    } 

}

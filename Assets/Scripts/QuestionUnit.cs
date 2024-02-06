public class QuestionUnit
{
    private string type;
    private string prompt;
    private string[] choices;
    private string[] answer;

    public QuestionUnit (string _type, string _prompt, string[] _choices, string[] _answer) 
    {
        type = _type;
        prompt = _prompt;
        choices = _choices;
        answer = _answer;
    }

    public string GetTypeString() 
    {
        return type;
    } 

    public string GetPromptString() 
    {
        return prompt;
    } 

    public string[] GetChoicesStrings() 
    {
        return choices;
    } 

    public string GetAnswerString() 
    {
        return answer[0];
    }

    public string[] GetAnswers() 
    {
        return answer;
    }

}

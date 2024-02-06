public class Question
{
    private QuestionUnit[] questionUnits;
    private int currentIndex;

    public Question (QuestionUnit[] _questionUnits) 
    {
        questionUnits = _questionUnits;
        ResetCurrentQUIndex();
    }    

    public QuestionUnit GetNextQU() 
    {
        return (++currentIndex < questionUnits.Length) ? questionUnits[currentIndex] : null;
    }

    public void ResetCurrentQUIndex() 
    {
        currentIndex = -1;
    }
    
    
}

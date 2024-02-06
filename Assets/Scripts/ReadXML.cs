using System.Xml;
using System.Collections.Generic;
using UnityEngine;
public static class ReadXML 
{
    private static Dictionary<string,QuestionUnitType> QUTypes;

    static ReadXML() 
    {
        QUTypes = new Dictionary<string, QuestionUnitType>();
        ReadQUTypes();
    }

    public static void ReadQUTypes() 
    {
        TextAsset quTypesXML = (TextAsset) (Resources.Load("XML/question_unit_types"));
        XmlDocument quTypesDoc = new XmlDocument();

        quTypesDoc.LoadXml(quTypesXML.text);

        XmlNode quType = quTypesDoc.FirstChild.NextSibling.FirstChild; //The first question unit type


        while(quType != null) 
        {
            XmlNode quTypeElement = quType.FirstChild;
            string typeName=quType.Attributes["type"].Value,promptType="", choiceType="", generalPrompt="",reminderFeedback="";
            string[] correctFeedbacks = null, incorrectFeedbacks = null;

            while (quTypeElement != null) 
            {
                switch (quTypeElement.Name) 
                {
                    case "prompt_type":
                    promptType = quTypeElement.InnerText.Trim();
                    break;

                    case "choice_type":
                    choiceType = quTypeElement.InnerText.Trim();
                    break;

                    case "general_prompt":
                    generalPrompt = quTypeElement.InnerText.Trim();
                    break;

                    case "correct_feedbacks":
                    correctFeedbacks = new string[quTypeElement.ChildNodes.Count];
                    XmlNode cf = quTypeElement.FirstChild;
                    int cfCounter = 0;

                    while (cf != null)
                    {
                        correctFeedbacks[cfCounter++] = cf.InnerText.Trim();
                        cf = cf.NextSibling;
                    }    
                    break;

                    case "incorrect_feedbacks":
                    incorrectFeedbacks = new string[quTypeElement.ChildNodes.Count];
                    XmlNode icf = quTypeElement.FirstChild;
                    int icfCounter = 0;

                    while (icf != null)
                    {
                        incorrectFeedbacks[icfCounter++] = icf.InnerText.Trim();
                        icf = icf.NextSibling;
                    }   
                    break;

                    case "reminder_feedback":
                    reminderFeedback = quTypeElement.InnerText.Trim();
                    break;

                }
                quTypeElement = quTypeElement.NextSibling;
            }

            QUTypes.Add(typeName, new QuestionUnitType(typeName,promptType,choiceType,generalPrompt,reminderFeedback,correctFeedbacks,incorrectFeedbacks));
            quType = quType.NextSibling;

        }


    }

    public static Question[] ReadQuestions(string questionsFileName) 
    {
        TextAsset questionsXML = (TextAsset) (Resources.Load("XML/"+questionsFileName));
        XmlDocument questionsDoc = new XmlDocument();

        questionsDoc.LoadXml(questionsXML.text);

        Question[] questions = new Question[questionsDoc.FirstChild.NextSibling.ChildNodes.Count];
        XmlNode q = questionsDoc.FirstChild.NextSibling.FirstChild; //The first question
        int questionCounter = 0;

        while(q != null) 
        {
            XmlNode qu = q.FirstChild; //The first qu of the question
            QuestionUnit[] questionUnits = new QuestionUnit[q.ChildNodes.Count];
            int quCounter = 0;
            while (qu != null) 
            {
                string type = qu.Attributes["type"].Value;
                string prompt = "";
                string[] choices = null, answer = null;

                XmlNode quElement = qu.FirstChild;
               
                while (quElement != null) 
                {
                    switch(quElement.Name) 
                    {
                        case "prompt":
                       
                        prompt = quElement.InnerText.Trim();;
                        break;

                        case "choices":
                        choices = new string[quElement.ChildNodes.Count];
                        XmlNode c = quElement.FirstChild;
                        int cCounter = 0;

                        while (c != null)
                        {
                            choices[cCounter++] = c.InnerText.Trim();;
                            c = c.NextSibling;
                        }
                        
                        break;

                        case "answers":
                        answer = new string[quElement.ChildNodes.Count];
                        XmlNode a = quElement.FirstChild;
                        int aCounter = 0;

                        while (a != null)
                        {
                            answer[aCounter++] = a.InnerText.Trim();;
                            a = a.NextSibling;
                        }
                        break;
                        
                    }
                    
                    quElement = quElement.NextSibling;
                }

                questionUnits[quCounter++] = new QuestionUnit(type,prompt,choices,answer);
                qu = qu.NextSibling; //The next qu of the question
            }

            questions[questionCounter++] = new Question(questionUnits);
            q = q.NextSibling; //The next question
        }

        return questions;
    }    

    public static QuestionUnitType GetQuestionUnitType (string typeName) 
    {
        return QUTypes[typeName];
    }
}

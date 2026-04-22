using System;
using System.Collections.Generic;

[Serializable]
public class TriviaQuestion
{
    public string type;
    public string difficulty;
    public string category;
    public string question;
    public string correct_answer;
    public List<string> incorrect_answers;
}
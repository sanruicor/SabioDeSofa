using System;
using System.Collections.Generic;

[Serializable]
public class ApiResponse
{
    public int response_code;
    public List<TriviaQuestion> results;
}
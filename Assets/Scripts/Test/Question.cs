using System.Collections.Generic;

namespace Test
{
    [System.Serializable]
    public class Question
    {
        public string QuestionText;
        public string[] Options;
        public int correctAnswerIndex;
    }
    public class QuestionList
    {
        public List<Question> Questions;
    }
}
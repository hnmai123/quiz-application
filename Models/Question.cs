using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public enum QuestionType
    {
        [Display(Name = "Multi Choice")]
        MultiChoice,
        [Display(Name = "Single Choice")]
        SingleChoice,
        [Display(Name = "Short Answer")]
        ShortAnswer
    }
    public class Question
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string QuestionText { get; private set; }
        public int MaxMark { get; private set; }
        public List<Answer> Answers { get; private set; } = new();
        public Guid QuizId { get; private set; }
        public Quiz? Quiz { get; private set; }
        public List<Answer> CorrectAnswers { get; private set; } = new();
        public QuestionType QuestionType { get; private set; }
        public Question(string questionText, int maxMark, QuestionType questionType, Guid quizId)
        {
            QuestionText = questionText;
            MaxMark = maxMark;
            QuestionType = questionType;
            QuizId = quizId;
        }
        public void AddAnswer(Answer answer, string response, bool isCorrect = false)
        {
            Answers.Add(new Answer(Id, response, isCorrect));
        }
        // Handle the case where the question type is ShortAnswer
        public IEnumerable<Answer> GetCorrectAnswers()
        {
            if (QuestionType == QuestionType.ShortAnswer)
            {
                return Enumerable.Empty<Answer>();

            }
            return Answers.Where(a => a.IsCorrect);
        }
    }
}
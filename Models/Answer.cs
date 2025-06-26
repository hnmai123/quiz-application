namespace QuizApp.Models
{
    public class Answer
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid QuestionId { get; private set; }
        public string Response { get; private set; }
        public bool IsCorrect { get; private set; }
        public Answer(Guid questionId, string response, bool isCorrect = false)
        {
            QuestionId = questionId;
            Response = response;
            IsCorrect = isCorrect;
        }
    }
}
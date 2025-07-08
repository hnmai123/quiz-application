namespace QuizApp.Models
{
    public class QuizAttempt
    {
        public Guid Id { get; set; }
        public Guid QuizId { get; set; }
        public Quiz? Quiz { get; set; }
        public int Score { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
        public List<UserAnswer> UserAnswers { get; set; } = new();
    }

    public class UserAnswer
    {
        public Guid Id { get; set; }
        public Guid QuizAttemptId { get; set; }
        public QuizAttempt? QuizAttempt { get; set; }
        public Guid QuestionId { get; set; }
        public required Question Question { get; set; }
        public string? TextAnswer { get; set; } // Short answer or essay
        public List<Guid> SelectedAnswerIds { get; set; } = new(); // For multiple choice questions
        public bool IsCorrect { get; set; } // To track if the answer was correct
    }
}